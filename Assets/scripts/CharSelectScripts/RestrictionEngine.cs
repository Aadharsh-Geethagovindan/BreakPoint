using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class RestrictionEngine
{
    private const bool DBG = false; // flip to false to silence logs
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
        var picks = rarities.Select(r => rarityRank[r]).OrderByDescending(x => x).ToList();
        //if (DBG) Debug.Log($"[Restrict] IsValidTeam picks={string.Join(",", picks)}");

        foreach (var pat in patterns)
        {
            var slots = pat.OrderBy(x => x).ToList(); // ASC
            var ok = BestFitAssign(picks, slots);     // mutates 'slots' to remaining
            //if (DBG) Debug.Log($"[Restrict]  pattern={string.Join(",", pat)} fit={ok} remaining={string.Join(",", slots)}");
            if (ok) return true;
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
        var picks = currentPicks.Select(r => rarityRank[r]).OrderByDescending(x => x).ToList();
        

        var allowed = new HashSet<int>();

        foreach (var pat in patterns)
        {
            var slots = pat.OrderBy(x => x).ToList();     // ASC
            var ok = BestFitAssign(picks, slots);         // mutates 'slots' to remaining
            

            if (!ok) continue;

            foreach (var cap in slots)
                for (int r = 0; r <= cap; r++)
                    allowed.Add(r);
        }

        

        return allowed;
    }
     private static bool BestFitAssign(List<int> picksDesc, List<int> slotsAsc)
    {
        foreach (var p in picksDesc)
        {
            int k = slotsAsc.FindIndex(s => s >= p);   // first slot that can hold this pick
            if (k < 0) return false;                   // no slot fits -> pattern fails
            slotsAsc.RemoveAt(k);                      // consume that slot
        }
        return true;
    }
}
