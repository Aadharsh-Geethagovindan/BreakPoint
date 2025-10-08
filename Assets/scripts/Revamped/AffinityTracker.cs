using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Essence = DamageType;

namespace Breakpoint.Revamped
{
    public class AffinityTracker
    {
        private readonly int teamId;
        private readonly RevampTuningConfig cfg;

        // track whether we already fired a fusion inside the current window
        private bool dualFiredThisWindow = false;      
        private bool tripleFiredThisWindow = false;    
        private int  windowStartRound = -1;            


        // Marks per Essence (Force/Elemental/Arcane/Corrupt). We ignore True/None.
        private readonly Dictionary<Essence, int> marks = new Dictionary<Essence, int>
        {
            { Essence.Force, 0 },
            { Essence.Elemental, 0 },
            { Essence.Arcane, 0 },
            { Essence.Corrupt, 0 },
        };

        // Which essences triggered within the fusion window (by round index)
        private readonly List<(Essence e, int round)> recentSingles = new List<(Essence, int)>();

        public AffinityTracker(int teamId, RevampTuningConfig config)
        {
            this.teamId = teamId;
            this.cfg = config;

            // Subscribe to events only in Revamped mode
            EventManager.Subscribe("OnAbilityResolved", OnAbilityResolved);   // NEW
            EventManager.Subscribe("OnRoundEnded", OnRoundEnded);        // NEW
        }

        public void Dispose()
        {
            EventManager.Unsubscribe("OnAbilityResolved", OnAbilityResolved);  // NEW
            EventManager.Unsubscribe("OnRoundEnded", OnRoundEnded);       // NEW
        }

        // --- Event handlers ---

        private void OnAbilityResolved(object payload)
        {
            // Expect: Source(GameCharacter), Targets(List<GameCharacter>), Ability(Ability),
            // Essence(DamageType), Outcome(OutcomeFlags), TeamId(int), AbilityType(AbilityType)
            var evt = payload as GameEventData;
            if (evt == null) return;

            // Route only our team’s events
            int srcTeam = evt.Get<int>("TeamId");
            if (srcTeam != teamId) return;

            var essence = evt.Get<Essence>("Essence");
            if (!IsTrackEssence(essence)) return; // ignore True/None/other

            // Base marks by ability type
            var ability = evt.Get<Ability>("Ability");
            int add = BaseMarksFor(ability);

            // Bonus marks from outcomes (stun/dot/buff/…)
            OutcomeFlags flags = OutcomeFlags.None;
            if (evt.Has("Outcome")) flags = evt.Get<OutcomeFlags>("Outcome");
            add += BonusMarksFrom(flags);

            if (add <= 0) return;

            AddMarks(essence, add);
        }

        private void OnRoundEnded(object payload)
        {
            var evt = payload as GameEventData;
            if (evt == null) return;

            int round = evt.Get<int>("Round");
            PruneOldSingles(round);
            TryFireDualOrTriple(round); // safety net end-of-round check
        }

        // --- Core ---

        private void AddMarks(Essence essence, int amount)
        {
            marks[essence] += amount;
            Debug.Log($"[Affinity] Team {teamId} +{amount} {essence} → {marks[essence]}/{cfg.singleTrackThreshold}");

            if (marks[essence] >= cfg.singleTrackThreshold)
            {
                marks[essence] -= cfg.singleTrackThreshold; // wrap around; spillover stays
                CoroutineRunner.Run(ApplySingleAfterDelay(essence)); // sequence with small delay

                // Record this single for fusion window
                int currentRound = TurnManager.Instance.GetCurrentRound(); // use your real accessor
                recentSingles.Add((essence, currentRound));
                // start (or keep) the active window when the first single hits
                if (windowStartRound < 0) windowStartRound = currentRound;

                if (cfg.checkFusionImmediately)
                {
                    PruneOldSingles(currentRound);
                    TryFireDualOrTriple(currentRound);
                }
            }
            EventManager.Trigger("OnTrackMarkAdded",
            new GameEventData()
                .Set("TeamId", teamId)
                .Set("Essence", essence)
                .Set("CurrentMarks", marks[essence])
                .Set("Threshold", cfg.singleTrackThreshold));
        }

        private IEnumerator ApplySingleAfterDelay(Essence essence)
        {
            yield return new WaitForSeconds(cfg.delay_Single);

            // Fire the single-track effect via events; your game systems will listen.
            switch (essence)
            {
                case Essence.Force:
                    EventManager.Trigger("OnTrack_Force", new GameEventData()
                        .Set("TeamId", teamId)
                        .Set("Duration", cfg.dur_Force_Stagger));
                    Debug.Log("Force Triggered");
                    break;

                case Essence.Elemental:
                    EventManager.Trigger("OnTrack_Elemental", new GameEventData()
                        .Set("TeamId", teamId)
                        .Set("Duration", cfg.dur_Elemental_Sustain));
                    Debug.Log("Elemental Triggered");
                    break;

                case Essence.Arcane:
                    EventManager.Trigger("OnTrack_Arcane", new GameEventData()
                        .Set("TeamId", teamId)
                        .Set("Duration", cfg.dur_Arcane_Tempo));
                    Debug.Log("Arcane Triggered");
                    break;

                case Essence.Corrupt:
                    EventManager.Trigger("OnTrack_Corrupt", new GameEventData()
                        .Set("TeamId", teamId)
                        .Set("Duration", cfg.dur_Corrupt_DoTAmp));
                    Debug.Log("Corrupt Triggered");
                    break;
            }
        }

        private void TryFireDualOrTriple(int currentRound)
        {
            // Collect distinct essences that triggered within window
            var window = cfg.fusionWindowTurns;
            var set = new HashSet<Essence>();
            for (int i = recentSingles.Count - 1; i >= 0; i--)
            {
                var (e, r) = recentSingles[i];
                if (currentRound - r <= window) set.Add(e);
            }
            
            if (set.Count >= 3 && !tripleFiredThisWindow)
            {
                tripleFiredThisWindow = true;                            
                dualFiredThisWindow = true;                               
                CoroutineRunner.Run(ApplyTripleAfterDelay());
                return;
                
            }
            else if (set.Count >= 2 && !dualFiredThisWindow )
            {
                dualFiredThisWindow = true;      
                // Fire the *first* pair found in the window; pairing logic to be improved later
                using var it = set.GetEnumerator();
                it.MoveNext(); var a = it.Current;
                it.MoveNext(); var b = it.Current;
                CoroutineRunner.Run(ApplyDualAfterDelay(a, b));
                
            }
        }

        private IEnumerator ApplyDualAfterDelay(Essence a, Essence b)
        {
            yield return new WaitForSeconds(cfg.delay_Dual);

            // Ensure stable ordering (Force+Arcane is same as Arcane+Force)
            if ((int)a > (int)b) { var t = a; a = b; b = t; }

            // Map the 6 pairs to their effects:
            if (a == Essence.Force && b == Essence.Elemental || b == Essence.Force && a == Essence.Elemental)  
            {
                EventManager.Trigger("OnDual_FE_Eruption", new GameEventData().Set("TeamId", teamId));
                Debug.Log($"Fusion Triggered between {a} and {b} for team{teamId}");
            }
            else if (a == Essence.Force && b == Essence.Arcane || b == Essence.Force && a == Essence.Arcane) 
            {
                EventManager.Trigger("OnDual_FA_Disruption", new GameEventData().Set("TeamId", teamId));
                Debug.Log($"Fusion Triggered between {a} and {b} for team{teamId}");
            }
            else if (a == Essence.Force && b == Essence.Corrupt || b == Essence.Force && a == Essence.Corrupt ) 
            {
                EventManager.Trigger("OnDual_FC_Crush", new GameEventData().Set("TeamId", teamId)); 
                Debug.Log($"Fusion Triggered between {a} and {b} for team{teamId}");
            }
            else if (a == Essence.Elemental && b == Essence.Arcane || b == Essence.Elemental && a == Essence.Arcane)
            {
                EventManager.Trigger("OnDual_EA_Purify", new GameEventData().Set("TeamId", teamId));
                Debug.Log($"Fusion Triggered between {a} and {b} for team{teamId}");
            }

            else if (a == Essence.Elemental && b == Essence.Corrupt || b == Essence.Elemental && a == Essence.Corrupt)
            {
                EventManager.Trigger("OnDual_EC_Blightstorm", new GameEventData().Set("TeamId", teamId));
                Debug.Log($"Fusion Triggered between {a} and {b} for team{teamId}");
            }
            else if (a == Essence.Arcane && b == Essence.Corrupt || b == Essence.Arcane && a == Essence.Corrupt)
            {
                EventManager.Trigger("OnDual_AC_Mindbreak", new GameEventData().Set("TeamId", teamId));
                Debug.Log($"Fusion Triggered between {a} and {b} for team{teamId}");
            }
        }

        private IEnumerator ApplyTripleAfterDelay()
        {
            yield return new WaitForSeconds(cfg.delay_Triple);

            // Unified Cataclysm
            EventManager.Trigger("OnTriple_Cataclysm", new GameEventData()
                .Set("TeamId", teamId)
                .Set("MaxHPPercent", cfg.cataclysm_MaxHPPercentLoss));
        }

        private void PruneOldSingles(int currentRound)
        {
            int i = 0;
            while (i < recentSingles.Count)
            {
                if (currentRound - recentSingles[i].round > cfg.fusionWindowTurns)
                    recentSingles.RemoveAt(i);
                else i++;
            }

            if (recentSingles.Count == 0 || 
            (windowStartRound >= 0 && currentRound - windowStartRound > cfg.fusionWindowTurns))
            {
                dualFiredThisWindow = false;      
                tripleFiredThisWindow = false;   
                windowStartRound = -1;            
            }
        }

        private static bool IsTrackEssence(Essence e)
        {
            return e == Essence.Force || e == Essence.Elemental || e == Essence.Arcane || e == Essence.Corrupt;
        }

        private int BaseMarksFor(Ability ability)
        {
            switch (ability.AbilityType) 
            {
                case AbilityType.Normal:      return cfg.marksOnBasic;
                case AbilityType.Skill:      return cfg.marksOnSkill;
                case AbilityType.Signature:  return cfg.marksOnSignature;
                default: return 0;
            }
        }

        private int BonusMarksFrom(OutcomeFlags flags)
        {
            int add = 0;
            if (flags.HasFlag(OutcomeFlags.Stun))   add += cfg.bonusMarks_Stun;
            if (flags.HasFlag(OutcomeFlags.Dot))    add += cfg.bonusMarks_Dot;
            if (flags.HasFlag(OutcomeFlags.Buff))   add += cfg.bonusMarks_Buff;
            if (flags.HasFlag(OutcomeFlags.Debuff)) add += cfg.bonusMarks_Debuff;
            if (flags.HasFlag(OutcomeFlags.Shield)) add += cfg.bonusMarks_Shield;
            if (flags.HasFlag(OutcomeFlags.Heal))   add += cfg.bonusMarks_Heal;
            return add;
        }
    }

    /// <summary>
    /// Tiny helper to run coroutines from non-Mono classes.
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _inst;
        public static void Ensure()
        {
            if (_inst == null)
            {
                var go = new GameObject("RevampCoroutineRunner");
                DontDestroyOnLoad(go);
                _inst = go.AddComponent<CoroutineRunner>();
            }
        }
        public static void Run(IEnumerator co)
        {
            Ensure();
            _inst.StartCoroutine(co);
        }
    }
}
