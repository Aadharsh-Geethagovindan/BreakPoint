using System.Collections.Generic;

public static class OnlineMatchData
{
    public struct RosterEntry
    {
        public int CharacterId;
        public int TeamId;
        public int SlotIndex;     // 0-2 within team
        public string CharacterName;
    }

    public static readonly List<RosterEntry> Roster = new();
    public static bool HasRoster => Roster.Count > 0;

    public static void Clear()
    {
        Roster.Clear();
    }
}
