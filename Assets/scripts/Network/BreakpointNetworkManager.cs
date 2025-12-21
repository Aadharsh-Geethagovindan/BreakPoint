using Mirror;
using UnityEngine;
using System.Collections.Generic;
public class BreakpointNetworkManager : NetworkManager
{
    [Header("Breakpoint")]
    public NetworkGameController networkGameControllerPrefab;
    private LocalMatchController _localMatchController;
    private ActiveCharPanel activeCharPanel;
    public int ConnectedClientCount => Mirror.NetworkServer.active ? Mirror.NetworkServer.connections.Count : 0;
    private bool _handlersRegistered = false;
    private readonly Dictionary<int, int> _connIdToTeam = new Dictionary<int, int>();


    private static BreakpointNetworkManager _instance;

    public override void Awake()
    {
        base.Awake();

        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        if (dontDestroyOnLoad) // whatever your bool is named in your script
            DontDestroyOnLoad(gameObject);
    }
    
    private string GetPortString()
    {
        if (transport is TelepathyTransport tp)
            return tp.port.ToString();
        return "unknown-port";
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log($"Server: started, listening on {networkAddress}:{GetPortString()}");
       // Handlers should exist regardless of scene
        EnsureServerHandlersRegistered();

        // Only initialize match controller if we're already in Arena
        TryInitMatchControllerIfArena();
        }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        Debug.Log("Server: client connected, connectionId = " + conn.connectionId);

        NetworkServer.SetClientReady(conn);

        int teamId = AssignTeamForConnection(conn);

        conn.Send(new AssignTeamNetMessage { TeamId = teamId });
        Debug.Log($"Server: assigned Team {teamId} to connectionId={conn.connectionId}");
    }

    public override void OnStartHost()
    {
        base.OnStartHost();
        OnlinePlayerIdentity.SetTeam(1);
        Logger.Instance?.PostLog("[Net] Host is TeamId=1", LogType.Status);
        Debug.Log("Host: TeamId=1");
    }

    private int AssignTeamForConnection(NetworkConnectionToClient conn)
    {
        // If already assigned (reconnect), return existing
        if (_connIdToTeam.TryGetValue(conn.connectionId, out int existing))
            return existing;

        // Host's local client connection should be Team 1
        if (conn == NetworkServer.localConnection)
        {
            _connIdToTeam[conn.connectionId] = 1;
            return 1;
        }

        // First remote client becomes Team 2 (2-player only)
        _connIdToTeam[conn.connectionId] = 2;
        return 2;
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log("Server: client disconnected, connectionId = " + conn.connectionId);
        base.OnServerDisconnect(conn);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"Client: starting, connecting to {networkAddress}:{GetPortString()}");

        NetworkClient.RegisterHandler<AssignTeamNetMessage>(OnAssignTeamMessage);
        NetworkClient.RegisterHandler<AbilityResultNetMessage>(OnAbilityResultMessage);
        NetworkClient.RegisterHandler<GameStateSnapshotNetMessage>(OnSnapshotMessage);
        NetworkClient.RegisterHandler<DraftStateNetMessage>(OnlineDraftClient.OnDraftStateMessage);
        NetworkClient.RegisterHandler<RosterMappingNetMessage>(OnRosterMappingMessage);


    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("Client: connected to server.");
    }

    public override void OnClientDisconnect()
    {
        Debug.Log("Client: disconnected from server.");
        OnlinePlayerIdentity.SetTeam(-1);
        base.OnClientDisconnect();
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        // Scene changes are where we should initialize server-side match logic
        TryInitMatchControllerIfArena();
    }

    private void EnsureServerHandlersRegistered()
    {
        if (_handlersRegistered) return;

        NetworkServer.RegisterHandler<UseAbilityNetMessage>(OnUseAbilityMessage, false);
        NetworkServer.RegisterHandler<SkipTurnNetMessage>(OnSkipTurnMessage, false);
        NetworkServer.RegisterHandler<PickRequestNetMessage>(OnPickRequestMessage, false);


        _handlersRegistered = true;
    }

    private void TryInitMatchControllerIfArena()
    {
        
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Arena")
            return;

        if (_localMatchController != null)
            return;

        if (BattleManager.Instance == null || TurnManager.Instance == null)
        {
            Debug.LogWarning("BreakpointNetworkManager: Arena loaded but BattleManager/TurnManager not ready yet.");
            return;
        }

        _localMatchController = new LocalMatchController(BattleManager.Instance, TurnManager.Instance);
        Debug.Log("BreakpointNetworkManager: LocalMatchController initialized (Arena).");
    }

    private void OnSnapshotMessage(GameStateSnapshotNetMessage msg)
    {
        var snap = msg.Snapshot;
        if (snap == null)
        {
            Debug.LogWarning("OnSnapshotMessage: snapshot is null.");
            return;
        }

        // Visible confirmation on BOTH editor and build
        Logger.Instance.PostLog($"[Net] Snapshot received. Round {snap.RoundNumber}. CurrentCharId {snap.CurrentCharacterId}",LogType.Status);
        activeCharPanel = Object.FindFirstObjectByType<ActiveCharPanel>();
        foreach (var cs in snap.Characters)
        {
            var card = activeCharPanel.FindCardForCharacterId(cs.Id); 
            if (card == null) continue;

            // HP Bar object is stored on the card as RectTransform HPBar
            var hpCtrl = card.HPBar != null ? card.HPBar.GetComponent<HPBarController>() : null;
            Logger.Instance.PostLog(
                $"cs.Id={cs.Id} hp={cs.HP}/{cs.MaxHP} card={(card != null)} bar={(card.HPBar != null)}",
                LogType.Status
            );
            if (hpCtrl != null)
                hpCtrl.ApplySnapshotHP(cs.HP, cs.MaxHP);

            card.RefreshStatusEffectsFromSnapshot(cs.StatusEffects);
        }
    }
    private void OnUseAbilityMessage(NetworkConnectionToClient conn, UseAbilityNetMessage msg)
    {
        if (_localMatchController == null)
        {
            Debug.LogWarning("OnUseAbilityMessage: _localMatchController is null.");
            return;
        }

        var cmd = UseAbilityCommand.FromData(msg.Data);
        if (cmd == null)
        {
            Debug.LogError("OnUseAbilityMessage: Failed to reconstruct UseAbilityCommand from data.");
            return;
        }

        Debug.Log("OnUseAbilityMessage: Executing UseAbilityCommand on server.");
        _localMatchController.HandleUseAbility(cmd);
    }

    private void OnSkipTurnMessage(NetworkConnectionToClient conn, SkipTurnNetMessage msg)
    {
        if (_localMatchController == null)
        {
            Debug.LogWarning("OnSkipTurnMessage: _localMatchController is null.");
            return;
        }

        var cmd = SkipTurnCommand.FromData(msg.Data) ?? new SkipTurnCommand(null);
        Debug.Log("OnSkipTurnMessage: Executing SkipTurnCommand on server.");
        _localMatchController.HandleSkipTurn(cmd);
    }

    private void OnAbilityResultMessage(AbilityResultNetMessage msg)
    {
        var result = msg.Result;
        if (result == null)
        {
            Debug.LogWarning("OnAbilityResultMessage: result is null.");
            return;
        }

        var summary = $"[Net] Caster {result.CasterId} used {result.AbilityType} on {result.Targets.Count} target(s).";
        foreach (var tr in result.Targets)
        {
            summary += $"\n  Target {tr.TargetId}: Hit={tr.Hit}, Damage={tr.Damage}, HPAfter={tr.HPAfter}";
        }

        Debug.Log(summary);
        Logger.Instance.PostLog(summary, LogType.Status);
    }

    private void OnAssignTeamMessage(AssignTeamNetMessage msg)
    {
        OnlinePlayerIdentity.SetTeam(msg.TeamId);
        Logger.Instance?.PostLog($"[Net] Assigned TeamId={msg.TeamId}", LogType.Status);
        Debug.Log($"Client: assigned TeamId={msg.TeamId}");
    }

    private void OnPickRequestMessage(NetworkConnectionToClient conn, PickRequestNetMessage msg)
    {
        // forwarded to the server-side draft controller in OnlineCharacterSelect scene
        NetworkDraftController.ServerInstance?.HandlePickRequest(conn, msg.CharacterName);
    }
    
    private void OnRosterMappingMessage(RosterMappingNetMessage msg)
    {
        OnlineMatchData.Clear();

        if (msg.Entries != null)
        {
            foreach (var e in msg.Entries)
            {
                OnlineMatchData.Roster.Add(new OnlineMatchData.RosterEntry
                {
                    CharacterId = e.CharacterId,
                    TeamId = e.TeamId,
                    SlotIndex = e.SlotIndex,
                    CharacterName = e.CharacterName
                });
            }
        }

        Logger.Instance?.PostLog($"[Net] Roster received: {OnlineMatchData.Roster.Count} entries", LogType.Status);
    }
}
