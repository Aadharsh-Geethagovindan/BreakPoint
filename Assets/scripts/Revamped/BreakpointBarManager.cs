using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class BreakpointBarManager : MonoBehaviour
{
    [SerializeField] private Breakpoint.Revamped.RevampTuningConfig cfg; //  (assign via Inspector or load in Awake)
    private BattleManager bm;                                            // 
    private float B = 0f;                                                //  // tug-of-war value ∈ [-CAP, +CAP]

    // Handlers we store so we can unsubscribe safely                   // 
    private Action<object> hDmg, hHeal, hShield, hStatus, hCrit, hRound;

    void Awake()
    {
        bm = BattleManager.Instance;            
        if (cfg == null)                                                
            cfg = Resources.Load<Breakpoint.Revamped.RevampTuningConfig>("RevampTuningConfig");
    }

    void OnEnable()
    {
        hDmg   = e => OnDamage(e);                                       
        hHeal  = e => OnHeal(e);                                         
        hShield= e => OnShield(e);                                       
        hStatus= e => OnStatusApplied(e);                                
        hCrit  = e => OnCrit(e);                                         
        hRound = e => OnRoundEnded(e);                                   

        EventManager.Subscribe("OnDamageDealt",   hDmg);                 
        EventManager.Subscribe("OnHealApplied",   hHeal);                
        EventManager.Subscribe("OnShieldApplied", hShield);              
        EventManager.Subscribe("OnStatusApplied", hStatus);              
        EventManager.Subscribe("OnCriticalHit",   hCrit);                
        EventManager.Subscribe("OnRoundEnded",    hRound);               
    }

    void OnDisable()
    {
        EventManager.Unsubscribe("OnDamageDealt",   hDmg);               
        EventManager.Unsubscribe("OnHealApplied",   hHeal);              
        EventManager.Unsubscribe("OnShieldApplied", hShield);            
        EventManager.Unsubscribe("OnStatusApplied", hStatus);            
        EventManager.Unsubscribe("OnCriticalHit",   hCrit);              
        EventManager.Unsubscribe("OnRoundEnded",    hRound);             
    }

    // ---------- Core math helpers ----------

    // Average MaxHP of alive characters; used to normalize raw numbers  
    private float Hbar()
    {
        var roster = bm != null ? bm.GetAllAliveCharacters() : null;    // implement GetAllAliveCharacters() if needed
        if (roster == null || roster.Count == 0) return 600f;
        return roster.Average(c => (float)c.MaxHP);
    }

    // Essence multiplier (DamageType used as Essence)                    
    private float EssenceMult(DamageType t)
    {
        switch (t)
        {
            case DamageType.Force:     return cfg.bp_M_Force;
            case DamageType.Elemental: return cfg.bp_M_Elemental;
            case DamageType.Arcane:    return cfg.bp_M_Arcane;
            case DamageType.Corrupt:   return cfg.bp_M_Corrupt;
            default: return 1f;
        }
    }

    // Apply signed, damped delta to B and check trigger                  
    private void ApplyDelta(int teamId, float G, DamageType essenceHint = DamageType.True)
    {
        // Direction: Team1 = +1, Team2 = -1                            
        int sign = (teamId == 1) ? 1 : -1;

        // Essence bias (optional); only if a meaningful hint is given    
        float mEss = EssenceMult(essenceHint);
        float Gbiased = G * mEss;

        // Damping: damp = (1 - |B|/CAP)^p                                // (damping)
        float damp = Mathf.Pow(1f - Mathf.Clamp01(Mathf.Abs(B) / cfg.bp_Cap), cfg.bp_DampPower);

        float dB = sign * Gbiased * damp;                                //  (signed & damped delta)
        float newB = Mathf.Clamp(B + dB, -cfg.bp_Cap, cfg.bp_Cap);       // (clamp to [-CAP, +CAP])

        B = newB;
        EventManager.Trigger("OnBreakpointUpdated", new GameEventData()
            .Set("Value", B)                                             // raw tug value
            .Set("Cap", cfg.bp_Cap)                                      // for UI
        );

        // Trigger if we reached either edge                              //  (trigger)
        if (Mathf.Abs(B) >= cfg.bp_Cap - 0.0001f)
        {
            int winnerTeam = (B > 0f) ? 1 : 2;
            
            EventManager.Trigger("OnBreakpointTriggered", new GameEventData()
                .Set("TeamId", winnerTeam)); 
            // Hard reset to center (simple policy)                       //  (reset)
            B = 0f;
            EventManager.Trigger("OnBreakpointUpdated", new GameEventData()
                .Set("Value", B)
                .Set("Cap", cfg.bp_Cap));
        }
    }

    // ---------- Event handlers (compute G and feed ApplyDelta) ----------

    private void OnDamage(object payload)
    {
        var d = payload as GameEventData;
        if (d == null) return;

        var src = d.Get<GameCharacter>("Source");
        int amount = d.Get<int>("Amount");
        var dtype = d.Has("Type") ? d.Get<DamageType>("Type") : DamageType.True;

        if (src == null || amount <= 0) return;

        // Normalize by average MaxHP                                      //  (normalization)
        float dHat = amount / Mathf.Max(1f, Hbar());

        // Core gain: α * d̂                                               // (weights)
        float G = cfg.bp_W_Damage * dHat;

        ApplyDelta(src.TeamId, G, dtype);
    }

    private void OnHeal(object payload)
    {
        var d = payload as GameEventData;
        if (d == null) return;

        var src = d.Get<GameCharacter>("Source");
        int amount = d.Get<int>("Amount");
        var dtype = d.Has("Type") ? d.Get<DamageType>("Type") : DamageType.True;

        if (src == null || amount <= 0) return;

        float hHat = amount / Mathf.Max(1f, Hbar());                      
        float G = cfg.bp_W_Heal * hHat;                                   

        // If you want essence bias for heals, you can get it from the last ability context later.
        ApplyDelta(src.TeamId, G, dtype);                  
    }

    private void OnShield(object payload)
    {
        var d = payload as GameEventData;
        if (d == null) return;

        var src = d.Get<GameCharacter>("Source");
        int amount = d.Get<int>("Amount");
        var dtype = d.Has("Type") ? d.Get<DamageType>("Type") : DamageType.True;

        if (src == null || amount <= 0) return;

        float sHat = amount / Mathf.Max(1f, Hbar());                      
        float G = cfg.bp_W_Shield * sHat;                              

        ApplyDelta(src.TeamId, G, dtype);                  
    }

    private void OnStatusApplied(object payload)
    {
        var d = payload as GameEventData;
        if (d == null) return;

        var src = d.Get<GameCharacter>("Source"); // may be null for system/applied copies
        var eff = d.Get<StatusEffect>("Effect");
        if (eff == null) return;

        // Discrete bonuses punch through damping                          // (discrete bonuses)
        float bonus = 0f;
        if (eff.Type == StatusEffectType.Stun)             bonus += cfg.bp_Bonus_Stun;
        if (eff.IsDebuff && eff.Name == "PurifyRemoved")   bonus += cfg.bp_Bonus_Cleanse; // if you emit an explicit record
        // In Blightstorm we can add a separate event or set a flag; for now leave to Blightstorm to emit its own bonus.

        if (bonus <= 0f || src == null) return;

        ApplyDelta(src.TeamId, bonus, eff.DamageType);
    }

    private void OnCrit(object payload)
    {
        var d = payload as GameEventData;
        if (d == null) return;

        var src = d.Get<GameCharacter>("Source");
        var dtype = d.Has("Type") ? d.Get<DamageType>("Type") : DamageType.True;
        if (src == null) return;

        float G = Mathf.Max(0f, cfg.bp_Bonus_CritAdd);                   
        ApplyDelta(src.TeamId, G, dtype);                      
    }

    private void OnRoundEnded(object payload)
    {
        // Decay toward center: B ← B * (1 - decay)                        //  (decay)
        float k = Mathf.Clamp01(1f - cfg.bp_DecayPerRound);
        B *= k;

        EventManager.Trigger("OnBreakpointUpdated", new GameEventData()
            .Set("Value", B)
            .Set("Cap", cfg.bp_Cap));
    }
}
