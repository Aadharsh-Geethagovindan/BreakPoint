using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class RevampEffectsManager : MonoBehaviour
{
    [SerializeField] private Breakpoint.Revamped.RevampTuningConfig cfg; // assign via code or Inspector

    private System.Action<object> hForce, hElem, hArc, hCor;
    private System.Action<object> hFE, hFA, hFC, hEA, hEC, hAC;
    private System.Action<object> hTriple;

    private BattleManager bm;

    void Awake()
    {
        bm = Object.FindFirstObjectByType<BattleManager>();
        if (cfg == null)
        {
            // Fallback: load from Resources if not assigned in Inspector
            cfg = Resources.Load<Breakpoint.Revamped.RevampTuningConfig>("RevampTuningConfig");
        }
    }

    void OnEnable()
    {
        // Singles
        hForce = e => OnForceSingle(e);
        hElem  = e => OnElementalSingle(e);
        hArc   = e => OnArcaneSingle(e);
        hCor   = e => OnCorruptSingle(e);

        EventManager.Subscribe("OnTrack_Force",     hForce);
        EventManager.Subscribe("OnTrack_Elemental", hElem);
        EventManager.Subscribe("OnTrack_Arcane",    hArc);
        EventManager.Subscribe("OnTrack_Corrupt",   hCor);

        // Duals
        hFE = e => OnDual_FE_Eruption(e);
        hFA = e => OnDual_FA_Disruption(e);   // stub
        hFC = e => OnDual_FC_Crush(e);        // stub
        hEA = e => OnDual_EA_Purify(e);
        hEC = e => OnDual_EC_Blightstorm(e);
        hAC = e => OnDual_AC_Mindbreak(e);

        EventManager.Subscribe("OnDual_FE_Eruption",     hFE);
        EventManager.Subscribe("OnDual_FA_Disruption",   hFA);
        EventManager.Subscribe("OnDual_FC_Crush",        hFC);
        EventManager.Subscribe("OnDual_EA_Purify",       hEA);
        EventManager.Subscribe("OnDual_EC_Blightstorm",  hEC);
        EventManager.Subscribe("OnDual_AC_Mindbreak",    hAC);

        // Triple
        hTriple = e => OnTriple_Cataclysm(e);
        EventManager.Subscribe("OnTriple_Cataclysm", hTriple);
    }

    void OnDisable()
    {
        EventManager.Unsubscribe("OnTrack_Force",     hForce);
        EventManager.Unsubscribe("OnTrack_Elemental", hElem);
        EventManager.Unsubscribe("OnTrack_Arcane",    hArc);
        EventManager.Unsubscribe("OnTrack_Corrupt",   hCor);

        EventManager.Unsubscribe("OnDual_FE_Eruption",     hFE);
        EventManager.Unsubscribe("OnDual_FA_Disruption",   hFA);
        EventManager.Unsubscribe("OnDual_FC_Crush",        hFC);
        EventManager.Unsubscribe("OnDual_EA_Purify",       hEA);
        EventManager.Unsubscribe("OnDual_EC_Blightstorm",  hEC);
        EventManager.Unsubscribe("OnDual_AC_Mindbreak",    hAC);

        EventManager.Unsubscribe("OnTriple_Cataclysm", hTriple);
    }

    // ---------- Helpers ----------

    private List<GameCharacter> Allies(int teamId)
    {
        return bm != null ? bm.GetTeam(teamId) : new List<GameCharacter>();
    }

    private List<GameCharacter> Enemies(int teamId)
    {
        int enemy = (teamId == 1) ? 2 : 1;
        return bm != null ? bm.GetTeam(enemy) : new List<GameCharacter>();
    }

    private int TeamIdFrom(object payload)
    {
        if (payload is GameEventData d && d.Has("TeamId"))
            return d.Get<int>("TeamId");
        return 1;
    }

    private void Log(string msg) => Logger.Instance?.PostLog(msg, LogType.Info);

    // ---------- Singles ----------

    private void OnForceSingle(object payload)//works
    {
        int team = TeamIdFrom(payload);
        var enemies = Enemies(team);

        foreach (var enemy in enemies)
        {
            if (enemy.IsDead) continue;

            var spdDebuff = new StatusEffect(
                "Staggered",
                StatusEffectType.SPDModifier,
                1,      // lasts 1 turn
                -0.25f, // -25% speed
                source: null,
                isDebuff: true,
                toDisplay: true
            );

            enemy.AddStatusEffect(spdDebuff);
        }
        EventManager.Trigger("OnStatusesChanged");
        Log($"[Revamped] Force single — Team {team} applied SPD debuff");
    }

    private void OnElementalSingle(object payload) //works
    {
        int team = TeamIdFrom(payload);
        var allies = Allies(team);

        foreach (var ally in allies)
        {
            int shield = Mathf.CeilToInt(ally.MaxHP * cfg.elementalShieldPercent);
            if (shield > 0) ally.AddShield(shield);

            int heal = Mathf.CeilToInt(ally.MaxHP * cfg.elementalHealPercent);
            if (heal > 0) ally.Heal(heal);
        }

        Log($"[Revamped] Elemental single — Team {team} gained shields{(cfg.elementalHealPercent>0 ? " + heals" : "")}");
    }

    private void OnArcaneSingle(object payload)
    {
        int team = TeamIdFrom(payload);
        var enemies = Enemies(team);

        foreach (var enemy in enemies)
        {
            if (enemy.IsDead) continue;

            // Add a cooldown modifier: +1 turn to skill CDs for 2 turns
            var cdDebuff = new StatusEffect(
                "Arcane Disrupt", 
                StatusEffectType.CDModifier, 
                2, // duration
                1, // value (+1 turn cooldown)
                source: null, 
                isDebuff: true,
                toDisplay: true
            )
            { 
                AffectedAbilityType = AbilityType.Skill 
            };

            enemy.AddStatusEffect(cdDebuff);
        }
        EventManager.Trigger("OnStatusesChanged");
        Log($"[Revamped] Arcane single — Team {team} applied skill CD debuff");
    }

    private void OnCorruptSingle(object payload) //works
    {
        int team = TeamIdFrom(payload);
        var allies = Allies(team);

        foreach (var ally in allies)
        {
            var amp = new StatusEffect(
                "Corrupt DoT Amp",                       
                StatusEffectType.Custom,                 
                cfg.dur_Corrupt_DoTAmp,                  
                0.5f,                                    
                source: null,
                isDebuff: false,
                toDisplay: false                         
            );
            ally.AddStatusEffect(amp);                  
        }

        Log($"[Revamped] Corrupt single — Team {team} DoT AMP +50% for {cfg.dur_Corrupt_DoTAmp} turns");
    }

    // ---------- Duals ----------

    private void OnDual_FE_Eruption(object payload) //works
    {
        int team = TeamIdFrom(payload);
        var enemies = Enemies(team);

        
        foreach (var enemy in enemies)
        {
            if (!enemy.IsDead)
            {
                int dmg = Mathf.CeilToInt(enemy.MaxHP * 0.08f); // temp 8% maxHP true
                enemy.TakeDamage(dmg, DamageType.True);
            }
        }

        Log($"[Revamped] Fusion: Eruption — Team {team} dealt AoE True dmg (placeholder)");
    }

    private void OnDual_FA_Disruption(object payload)
    {
        int team = TeamIdFrom(payload);
        Log($"[Revamped] Fusion: Disruption (stub) — Team {team}");
        // TODO: Breakpoint bar reduction once bar manager exists.
    }

    private void OnDual_FC_Crush(object payload) // works
    {
        int team = TeamIdFrom(payload);
        var allies = Allies(team);

        foreach (var ally in allies)
        {
            var keen = new StatusEffect(
                "Keen Edge (Crush)", 
                StatusEffectType.CritRateModifier, 
                1,         // duration: 1 turn
                0.20f,     // +20% crit chance
                source: null, 
                isDebuff: false, 
                toDisplay: true
            );
            ally.AddStatusEffect(keen);
        }
        EventManager.Trigger("OnStatusesChanged");
        Log($"[Revamped] Fusion: Crush — Team {team} +20% Crit Rate (1 turn)");
    }

    private void OnDual_EA_Purify(object payload) // works
    {
        int team = TeamIdFrom(payload);
        var allies = Allies(team);

        foreach (var ally in allies)
        {
            // Gather all debuffs on this ally
            var debuffs = ally.StatusEffects.Where(e => e.IsDebuff).ToList();

            foreach (var debuff in debuffs)
            {
                ally.RemoveStatusEffect(debuff);
                Debug.Log($"Purify removed '{debuff.Name}' from {ally.Name}");
                EventManager.Trigger("OnFusionTriggered", new GameEventData()
                    .Set("SourceTeam", team)
                    .Set("Target", ally)
                    .Set("Description", $"Purify removed '{debuff.Name}'")
                );
            }
        }
        EventManager.Trigger("OnStatusesChanged"); // Optimized variant would be to confirm change did happen
        Log($"[Revamped] Fusion: Purify — Team {team} cleansed all debuffs");
    }

   private void OnDual_EC_Blightstorm(object payload) // works, ui icon update should happen instantly
    {
        int team = TeamIdFrom(payload);
        var enemies = Enemies(team).Where(e => !e.IsDead).ToList();

        // Nothing to do if <2 enemies alive
        if (enemies.Count < 2)
        {
            Log($"[Revamped] Fusion: Blightstorm — no valid spread targets");
            return;
        }

        // Helper: what counts as a spreadable debuff?
        bool IsSpreadable(StatusEffect se)
        {
            if (!se.IsDebuff) return false;
            // Exclude hard CC: Stun 
            if (se.Type == StatusEffectType.Stun) return false;
            return true;
        }

        // 1) Build a list of (debuff, carriers)
        var byDebuff = new Dictionary<string, List<GameCharacter>>(); // key by (Type|Name) so we don’t collide
        var sampleByKey = new Dictionary<string, StatusEffect>();

        foreach (var enemy in enemies)
        {
            foreach (var se in enemy.StatusEffects)
            {
                if (!IsSpreadable(se)) continue;
                string key = $"{se.Type}|{se.Name}";
                if (!byDebuff.TryGetValue(key, out var list))
                {
                    list = new List<GameCharacter>();
                    byDebuff[key] = list;
                    sampleByKey[key] = se; // keep one example to clone from
                }
                list.Add(enemy);
            }
        }

        if (byDebuff.Count == 0)
        {
            Log($"[Revamped] Fusion: Blightstorm — no spreadable debuffs found");
            return;
        }

        // 2) Pick a target debuff to spread:
        //    Priority A: debuffs that are on exactly ONE enemy (unique pressure)
        //    Fallback  : any spreadable debuff
        string pickKey = null;

        var uniques = byDebuff.Where(kv => kv.Value.Count == 1).Select(kv => kv.Key).ToList();
        if (uniques.Count > 0)
        {
            // Choose the first unique (or random if you prefer)
            pickKey = uniques[0];
        }
        else
        {
            // Choose any (or random) spreadable debuff key
            pickKey = byDebuff.Keys.First();
        }

        var carriers = byDebuff[pickKey];         // enemies that already have it
        var template = sampleByKey[pickKey];      // the debuff we will clone

        // 3) Spread to enemies that don’t already have this debuff (by same Type+Name)
        int applied = 0;
        foreach (var enemy in enemies)
        {
            if (carriers.Contains(enemy)) continue; // already has it

            bool alreadyHasSame = enemy.StatusEffects.Any(se => se.Type == template.Type && se.Name == template.Name);
            if (alreadyHasSame) continue;

            // Clone from template; keep remaining duration/value/dmg type/etc.
            var clone = new StatusEffect(template);               // uses your copy ctor
            //clone.Source = null;                                  // neutral source is fine here
            clone.ToDisplay = true;                               // show it (optional)
            enemy.AddStatusEffect(clone);
            applied++;

            EventManager.Trigger("OnStatusApplied", new GameEventData()
                .Set("Source", null)
                .Set("Target", enemy)
                .Set("Effect", clone));
        }
        if (applied > 0)
        {
            EventManager.Trigger("OnStatusesChanged");
        }

        Log(applied > 0
            ? $"[Revamped] Fusion: Blightstorm — spread '{template.Name}' to {applied} enemies"
            : $"[Revamped] Fusion: Blightstorm — selected '{template.Name}', but nothing to spread");
    }


    private void OnDual_AC_Mindbreak(object payload) //works
    {
        int team = TeamIdFrom(payload);
        var enemies = Enemies(team);
        var allies  = Allies(team);

        // enemy -30 sig, allies +20 sig (tweak later or move to knobs)
        foreach (var e in enemies) e.ReduceCharge(30);
        foreach (var a in allies)  a.IncreaseCharge(+20);

        Log($"[Revamped] Fusion: Mindbreak — Team {team} (-enemy sig / +ally sig )");
    }

    // ---------- Triple ----------

    private void OnTriple_Cataclysm(object payload)
    {
        int team = TeamIdFrom(payload);
        var enemies = Enemies(team);

        foreach (var enemy in enemies)
        {
            if (!enemy.IsDead)
            {
                int dmg = Mathf.CeilToInt(enemy.MaxHP * cfg.cataclysm_MaxHPPercentLoss);
                enemy.TakeDamage(dmg, DamageType.True);
            }
        }

        Log($"[Revamped] CATA CLYSM — Team {team} dealt {Mathf.RoundToInt(cfg.cataclysm_MaxHPPercentLoss*100)}% MaxHP True dmg");
    }
}
