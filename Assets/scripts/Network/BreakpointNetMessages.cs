using Mirror;

// Client → Server
public struct UseAbilityNetMessage : NetworkMessage
{
    public UseAbilityCommandData Data;
}

public struct SkipTurnNetMessage : NetworkMessage
{
    public SkipTurnCommandData Data;
}

// Server → Clients
public struct AbilityResultNetMessage : NetworkMessage
{
    public AbilityResult Result;
}

public struct GameStateSnapshotNetMessage : NetworkMessage
{
    public GameStateSnapshot Snapshot;
}

public struct AssignTeamNetMessage : NetworkMessage
{
    public int TeamId; // 1 or 2
}

public struct PickRequestNetMessage : NetworkMessage
{
    public string CharacterName; // use CharacterData.name as the key for now
}

public struct DraftStateNetMessage : NetworkMessage
{
    public int CurrentPickerTeamId; // 1 or 2
    public string[] P1Picks;
    public string[] P2Picks;
    public string[] Picked; // all picked names (for disabling grid)
}

public struct RosterMappingNetMessage : NetworkMessage
{
    public RosterEntryNet[] Entries;

    public struct RosterEntryNet
    {
        public int CharacterId;
        public int TeamId;
        public int SlotIndex;
        public string CharacterName;
    }
}