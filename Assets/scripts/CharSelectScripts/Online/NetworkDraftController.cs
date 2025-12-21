using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class NetworkDraftController : MonoBehaviour
{
    public static NetworkDraftController ServerInstance { get; private set; }

    private readonly List<string> _p1 = new();
    private readonly List<string> _p2 = new();
    private readonly HashSet<string> _picked = new();

    private int _currentPickerTeamId = 1; // Team 1 starts
    private const int MaxPerTeam = 3;
    private bool _sentRoster = false;

    private void Awake()
    {
        // server-only singleton
        if (NetworkServer.active)
            ServerInstance = this;
    }

    private void Start()
    {
        if (!NetworkServer.active) return;

        // broadcast initial state once the scene is live
        BroadcastStateToAll();
    }

    public void HandlePickRequest(NetworkConnectionToClient conn, string characterName)
    {
        if (!NetworkServer.active) return;
        if (string.IsNullOrWhiteSpace(characterName)) return;

        int teamId = GetTeamId(conn);

        // Turn validation
        if (teamId != _currentPickerTeamId)
            return;

        // Already picked validation
        if (_picked.Contains(characterName))
            return;

        // Capacity validation
        if (teamId == 1 && _p1.Count >= MaxPerTeam) return;
        if (teamId == 2 && _p2.Count >= MaxPerTeam) return;

        // Commit
        if (teamId == 1) _p1.Add(characterName);
        else _p2.Add(characterName);

        _picked.Add(characterName);

        // Advance turn if draft not complete
        if (!IsDraftComplete())
            _currentPickerTeamId = (_currentPickerTeamId == 1) ? 2 : 1;

        BroadcastStateToAll();

        if (!_sentRoster && IsDraftComplete())
        {
            _sentRoster = true;
            SendRosterAndStartMatch();
        }
    }

    private bool IsDraftComplete()
        => _p1.Count >= MaxPerTeam && _p2.Count >= MaxPerTeam;

    private void BroadcastStateToAll()
    {
        var msg = new DraftStateNetMessage
        {
            CurrentPickerTeamId = _currentPickerTeamId,
            P1Picks = _p1.ToArray(),
            P2Picks = _p2.ToArray(),
            Picked = _picked.ToArray()
        };

        NetworkServer.SendToAll(msg);
    }

    private void SendRosterAndStartMatch()
    {
        // Build deterministic roster mapping
        var entries = new List<RosterMappingNetMessage.RosterEntryNet>();

        // Team 1: ids 1..3
        for (int i = 0; i < _p1.Count; i++)
        {
            entries.Add(new RosterMappingNetMessage.RosterEntryNet
            {
                TeamId = 1,
                SlotIndex = i,
                CharacterId = 1 + i,
                CharacterName = _p1[i]
            });
        }

        // Team 2: ids 4..6
        for (int i = 0; i < _p2.Count; i++)
        {
            entries.Add(new RosterMappingNetMessage.RosterEntryNet
            {
                TeamId = 2,
                SlotIndex = i,
                CharacterId = 4 + i,
                CharacterName = _p2[i]
            });
        }

        var rosterMsg = new RosterMappingNetMessage { Entries = entries.ToArray() };

        NetworkServer.SendToAll(rosterMsg);

        // Change scene next frame to ensure clients process the message first
        StartCoroutine(ChangeSceneNextFrame());
    }

    private System.Collections.IEnumerator ChangeSceneNextFrame()
    {
        yield return null;
        // Uses NetworkManager's scene management
        var nm = Mirror.NetworkManager.singleton;
        nm.ServerChangeScene("Arena");
    }


    private int GetTeamId(NetworkConnectionToClient conn)
    {
        // Host client already has OnlinePlayerIdentity = 1 locally, but server needs a rule too.
        // For now: local connection = team 1; remote connection(s) = team 2.
        if (conn == NetworkServer.localConnection) return 1;
        return 2;
    }
}
