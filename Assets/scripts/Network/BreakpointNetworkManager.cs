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

    private string _lastTurnOrderKey = "";
    private static BreakpointNetworkManager _instance;
    private int _localTeamId = -1;            
    //private bool _canActThisSnapshot = false;
    private VolleyStartNetMessage? _pendingVolley;
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
        NetworkEventBridge.Wire();
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
        NetworkEventBridge.Wire();

        NetworkClient.RegisterHandler<ReplicatedEventNetMessage>(OnReplicatedEventMessage);
        NetworkClient.RegisterHandler<AssignTeamNetMessage>(OnAssignTeamMessage);
        NetworkClient.RegisterHandler<AbilityResultNetMessage>(OnAbilityResultMessage);
        NetworkClient.RegisterHandler<GameStateSnapshotNetMessage>(OnSnapshotMessage);
        NetworkClient.RegisterHandler<DraftStateNetMessage>(OnlineDraftClient.OnDraftStateMessage);
        NetworkClient.RegisterHandler<RosterMappingNetMessage>(OnRosterMappingMessage);
        NetworkClient.RegisterHandler<VolleyStartNetMessage>(OnVolleyStartMessage);
        NetworkClient.RegisterHandler<LogNetMessage>(OnLogNetMessage);

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

        //Logger.Instance.PostLog( $"[Net] Snapshot received. Round {snap.RoundNumber}. CurrentCharId {snap.CurrentCharacterId}", LogType.Status);

        if (activeCharPanel == null) activeCharPanel = Object.FindFirstObjectByType<ActiveCharPanel>();
        if (activeCharPanel == null || !activeCharPanel.HasCardsRegistered()) return;
        if (BattleManager.Instance == null) return;

        _localTeamId = OnlinePlayerIdentity.LocalTeamId;

        // Turn order animation sync (client only)
        if (Mirror.NetworkClient.active && MatchTypeService.IsOnline &&
            snap.TurnOrderIds != null && snap.TurnOrderIds.Count > 0)
        {
            string key = string.Join(",", snap.TurnOrderIds);
            if (key != _lastTurnOrderKey)
            {
                _lastTurnOrderKey = key;

                var tm = TurnManager.Instance;
                if (tm != null)
                {
                    tm.ApplyAuthoritativeTurnOrder(snap.TurnOrderIds);

                    if (AnimationManager.Instance != null)
                        StartCoroutine(AnimationManager.Instance.AnimateCardRepositioning(tm.GetTurnOrder()));
                }
            }
        }

        // HP + statuses
        foreach (var cs in snap.Characters)
        {
            var card = activeCharPanel.FindCardForCharacterId(cs.Id);
            if (card == null) continue;

            var hpCtrl = card.HPBar != null ? card.HPBar.GetComponent<HPBarController>() : null;
            if (hpCtrl != null)
                hpCtrl.ApplySnapshotHP(cs.HP, cs.MaxHP);

            card.RefreshStatusEffectsFromSnapshot(cs.StatusEffects);
        }
        if (snap.CurrentCharacterId >= 0)
        { 
            // Turn owner highlight (all clients)
            activeCharPanel.ClearActiveTurnVisuals(); 
            var activeCard = activeCharPanel.FindCardForCharacterId(snap.CurrentCharacterId);
            if (activeCard != null)
                activeCard.SetActiveTurnVisual(true);
        }
        // Active card + gating (client only)
        if (Mirror.NetworkClient.active && MatchTypeService.IsOnline)
        {
            if (_localTeamId <= 0) return;

            bool isMyTurn = snap.CurrentTeamId == _localTeamId;
            activeCharPanel.SetInteractable(isMyTurn);
            activeCharPanel.SetCanInteract(isMyTurn);

            int displayId = isMyTurn
                ? snap.CurrentCharacterId
                : FindNextCharacterIdForTeam(snap, _localTeamId);

            var displayCharacter = BattleManager.Instance.GetCharacterById(displayId);
            if (displayCharacter != null)
                activeCharPanel.DisplayCharacter(displayCharacter);

            activeCharPanel.SetInteractable(isMyTurn);
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
            summary += $"\n  Target {tr.TargetId}: Hit={tr.Hit}, Damage={tr.Damage}, HPAfter={tr.HPAfter}";

        Debug.Log(summary);
        //if (Logger.Instance != null) Logger.Instance.PostLog(summary, LogType.Status);
        // Client-only: rebuild OnAbilityUsed event with correct keys
 
        if (MatchTypeService.IsOnline && Mirror.NetworkClient.active && !Mirror.NetworkServer.active)
        {
            var bm = BattleManager.Instance;
            if (bm != null)
            {
                var user = bm.GetCharacterById(result.CasterId);
                if (user != null)
                {
                    var ability = user.GetAbilityOfType(result.AbilityType);
                    if (ability != null)
                    {
                        var targets = new List<GameCharacter>(result.Targets.Count);
                        foreach (var tr in result.Targets)
                        {
                            var t = bm.GetCharacterById(tr.TargetId);
                            if (t != null) targets.Add(t);
                        }

                        var evt = new GameEventData()
                            .Set("User", user)
                            .Set("Ability", ability)
                            .Set("Targets", targets);

                        EventManager.Trigger("OnAbilityUsed", evt);
                    }
                }
            }
        }

    }
    private void OnLogNetMessage(LogNetMessage msg)
    {
        if (Logger.Instance == null) return;

        // local-only replay (no re-broadcast)
        Logger.Instance.PostLogFromNetwork(msg.Message, (LogType)msg.Type);
    }
    private void OnReplicatedEventMessage(ReplicatedEventNetMessage msg)
    {
        // Only replay on pure clients; host already has local events firing
        if (Mirror.NetworkServer.active) return;

       try
        {
            NetworkEventBridge.ReplayOnClient(msg);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ReplicatedEvent replay failed for {msg.EventName}: {ex}");
            // swallow to avoid disconnect
        }
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

        //Logger.Instance?.PostLog($"[Net] Roster received: {OnlineMatchData.Roster.Count} entries", LogType.Status);
    }

    private int FindNextCharacterIdForTeam(GameStateSnapshot snap, int teamId)
    {
        if (snap.TurnOrderIds == null || snap.TurnOrderIds.Count == 0) return -1;

        // Build quick lookup: characterId -> teamId, isDead
        var teamById = new Dictionary<int, int>();
        var deadById = new HashSet<int>();
        foreach (var cs in snap.Characters)
        {
            teamById[cs.Id] = cs.TeamId;
            if (cs.IsDead) deadById.Add(cs.Id);
        }

        int startIndex = snap.TurnOrderIds.IndexOf(snap.CurrentCharacterId);
        if (startIndex < 0) startIndex = 0;

        // Scan forward, wrap
        for (int i = 1; i <= snap.TurnOrderIds.Count; i++)
        {
            int idx = (startIndex + i) % snap.TurnOrderIds.Count;
            int id = snap.TurnOrderIds[idx];

            if (deadById.Contains(id)) continue;
            if (teamById.TryGetValue(id, out var t) && t == teamId)
                return id;
        }

        return -1;
    }

    private void OnVolleyStartMessage(VolleyStartNetMessage msg)
    {
        // Only for online mode
        if (!MatchTypeService.IsOnline) return;

        // Host already plays the volley server-side; don't double-play
        if (NetworkServer.active) return;

        if (!NetworkClient.active) return;

        // Need UI cards registered to animate toward them
        var panel = Object.FindFirstObjectByType<ActiveCharPanel>();
        if (panel == null || !panel.HasCardsRegistered())
        {
            _pendingVolley = msg;
            StartCoroutine(PlayPendingVolleyWhenReady());
            return;
        }

        PlayVolleyFromMessage(msg);
    }

    private System.Collections.IEnumerator PlayPendingVolleyWhenReady()
    {
        // if multiple arrive quickly, last one wins; that's fine for now
        while (true)
        {
            var panel = Object.FindFirstObjectByType<ActiveCharPanel>();
            if (panel != null && panel.HasCardsRegistered())
                break;

            yield return null;
        }

        if (_pendingVolley.HasValue)
        {
            var msg = _pendingVolley.Value;
            _pendingVolley = null;
            PlayVolleyFromMessage(msg);
        }
    }

    private void PlayVolleyFromMessage(VolleyStartNetMessage msg)
    {
        var bm = BattleManager.Instance;
        if (bm == null) return;

        var caster = bm.GetCharacterById(msg.CasterId);
        if (caster == null) return;

        if (msg.TargetIds == null || msg.WillHit == null) return;
        int n = System.Math.Min(msg.TargetIds.Length, msg.WillHit.Length);
        if (n <= 0) return;

        var resolutions = new System.Collections.Generic.List<HitResolution>(n);
        for (int i = 0; i < n; i++)
        {
            var target = bm.GetCharacterById(msg.TargetIds[i]);
            if (target == null) continue;

            resolutions.Add(new HitResolution(target, msg.WillHit[i]));
        }

        if (resolutions.Count == 0) return;
        if (AnimationManager.Instance == null) return;

        StartCoroutine(AnimationManager.Instance.PlayVolley(caster, resolutions, msg.DamageType));
    }
}
