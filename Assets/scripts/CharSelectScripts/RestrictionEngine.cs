using System.Collections.Generic;
using System.Linq;

public static class RestrictionEngine
{
    // Map rarity string to rank (higher = rarer)
    private static readonly Dictionary<string, int> rarityRank = new Dictionary<string, int>
    {
        { "L", 4 },
        { "UR", 3 },
        { "R", 2 },
        { "UC", 1 },
        { "C", 0 }
    };

    // Allowed team patterns (max rank slots, order doesn't matter)
    private static readonly List<int[]> patterns = new List<int[]>
    {
        new int[] {4, 2, 1},  // L, ≤R, ≤UC
        new int[] {3, 3, 1},  // UR, UR, ≤UC
        new int[] {3, 2, 2},  // UR, R, R
    };

    public static bool IsValidTeam(List<string> rarities)
    {
        var ranks = rarities.Select(r => rarityRank[r]).OrderByDescending(x => x).ToList();

        foreach (var pattern in patterns)
        {
            var slots = pattern.OrderByDescending(x => x).ToList();
            if (Fits(ranks, slots)) return true;
        }

        return false;
    }

    private static bool Fits(List<int> picks, List<int> slots)
    {
        if (picks.Count > slots.Count) return false;

        // Greedy: sorted picks vs slots
        int i = 0, j = 0;
        while (i < picks.Count && j < slots.Count)
        {
            if (picks[i] <= slots[j])
            {
                i++; j++; // fit this pick into this slot
            }
            else
            {
                j++; // try next slot
            }
        }

        return i == picks.Count;
    }

    public static HashSet<int> AllowedNextRarities(List<string> currentPicks)
    {
        var ranks = currentPicks.Select(r => rarityRank[r]).OrderByDescending(x => x).ToList();
        var allowed = new HashSet<int>();

        foreach (var pattern in patterns)
        {
            var slots = pattern.OrderByDescending(x => x).ToList();

            // Try to fit current picks into this pattern
            int i = 0, j = 0;
            while (i < ranks.Count && j < slots.Count)
            {
                if (ranks[i] <= slots[j]) { i++; j++; }
                else { j++; }
            }
            if (i != ranks.Count) continue; // this pattern can't fit current picks

            // Collect remaining slots for this viable pattern
            for (; j < slots.Count; j++)
            {
                int maxRank = slots[j];
                for (int r = 0; r <= maxRank; r++)
                    allowed.Add(r);
            }
        }

        return allowed;
    }
}
