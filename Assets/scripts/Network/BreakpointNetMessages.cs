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

public struct VolleyStartNetMessage : NetworkMessage
{
    public int CasterId;
    public AbilityType AbilityType;   // optional but useful for debugging
    public DamageType DamageType;     // so client doesn't need to look up ability
    public int[] TargetIds;
    public bool[] WillHit;
}

public struct ReplicatedEventNetMessage : NetworkMessage
{
    public string EventName;

    public ReplicatedEventPayloadType PayloadType;

    public int SourceId;     // -1 if unused
    public int TargetId;     // -1 if unused
    public int Amount;       // 0 if unused
    public AbilityType AbilityType;
    public string Text;      // null/empty if unused
}

public struct LogNetMessage : NetworkMessage
{
    public string Message;
    public int Type; // cast from your LogType enum
}