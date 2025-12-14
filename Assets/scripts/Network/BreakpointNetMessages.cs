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