using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class BreakpointEffectManager : MonoBehaviour
{
    
    private void Awake()
    {
        EventManager.Subscribe("OnBreakpointChoiceSelected", HandleChoice);
        
    }

    private void OnDestroy()
    {
        EventManager.Unsubscribe("OnBreakpointChoiceSelected", HandleChoice);
    }

    private void HandleChoice(object data)
    {
        if (data is not GameEventData evt) return;

        int teamId = evt.Get<int>("TeamId");
        string choice = evt.Get<string>("Choice");
        string result = evt.Get<string>("Result");

        if (string.IsNullOrEmpty(result))
        {
            // Fallback if the animator didn't pass the text
            if (choice == "Buff")
                ApplyRandomBuff(teamId);
            else
                ApplyRandomDebuff(teamId);
            return;
        }

        // 🔹 Normalize the result text for easier comparison
        string key = result.ToLowerInvariant();

        // 🔹 Map result string → effect
        if (key.Contains("essence surge"))
            EssenceSurge(teamId);
        else if (key.Contains("barrier pulse"))
            BarrierPulse(teamId);
        else if (key.Contains("critical flow"))
            CriticalFlow(teamId);
        else if (key.Contains("sig overcharge"))
            SigOvercharge(teamId);
        else if (key.Contains("essence drain"))
            EssenceDrain(teamId);
        else if (key.Contains("withering weight"))
            WitheringWeight(teamId);
        else if (key.Contains("sig disrupt"))
            SigDisrupt(teamId);
        else if (key.Contains("weakened resolve"))
            WeakenedResolve(teamId);
        else
            Debug.LogWarning($"[Breakpoint] No effect mapped for '{result}'");
    }

    private void ApplyRandomBuff(int teamId)
    {
        var options = new System.Action<int>[] {
            EssenceSurge, BarrierPulse, CriticalFlow, SigOvercharge
        };
        options[Random.Range(0, options.Length)](teamId);
        TurnManager.Instance.RefreshAllStatusIcons();
    }

    private void ApplyRandomDebuff(int teamId)
    {
        var options = new System.Action<int>[] {
            EssenceDrain, WitheringWeight, SigDisrupt, WeakenedResolve
        };
        options[Random.Range(0, options.Length)](teamId);
    }

    // ---------------------- Buffs ----------------------

    private void EssenceSurge(int teamId)
    {
        foreach (var ally in GetAllies(teamId))
        {
            
            var eff = new StatusEffect("Essence Surge", StatusEffectType.DamageModifier, 2, 0.25f, null);
            ally.AddStatusEffect(eff);
        }
        Logger.Instance.PostLog("Essence Surge activated: +25% damage to allies!", LogType.Buff);
    }

    private void BarrierPulse(int teamId)
    {
        foreach (var ally in GetAllies(teamId))
        {
            
            int shieldValue = Mathf.RoundToInt(ally.MaxHP * 0.3f);
            ally.AddShield(shieldValue);
        }
        Logger.Instance.PostLog("Barrier Pulse activated: 30% Max HP shield granted!", LogType.Buff);
    }

    private void CriticalFlow(int teamId)
    {
        foreach (var ally in GetAllies(teamId))
        {
            
            var eff = new StatusEffect("Critical Flow", StatusEffectType.CritRateModifier, 2, 0.3f, null);
            ally.AddStatusEffect(eff);
        }
        Logger.Instance.PostLog("Critical Flow activated: +30% Crit chance!", LogType.Buff);
    }

    private void SigOvercharge(int teamId)
    {
        foreach (var ally in GetAllies(teamId))
        {
            
            ally.IncreaseCharge(40);
        }

        Logger.Instance.PostLog("Sig Overcharge activated: +40 Sig Charge!", LogType.Buff);
    }

    // ---------------------- Debuffs ----------------------

    private void EssenceDrain(int teamId)
    {
        foreach (var enemy in GetEnemies(teamId))
        {
            var eff = new StatusEffect("Essence Drain", StatusEffectType.DamageModifier, 2, -0.25f, null, isDebuff: true);
            enemy.AddStatusEffect(eff);
        }
        Logger.Instance.PostLog("Essence Drain activated: Enemies -25% Damage!", LogType.Debuff);
    }

    private void WitheringWeight(int teamId)
    {
        foreach (var enemy in GetEnemies(teamId))
        {
            var eff = new StatusEffect("Withering Weight", StatusEffectType.SPDModifier, 2, -0.2f, null, isDebuff: true);
            enemy.AddStatusEffect(eff);
        }
        Logger.Instance.PostLog("Withering Weight activated: Enemies -20% Speed!", LogType.Debuff);
    }

    private void SigDisrupt(int teamId)
    {
        foreach (var enemy in GetEnemies(teamId))
            enemy.ReduceCharge(30);

        Logger.Instance.PostLog("Sig Disrupt activated: Enemies lose 30 Sig Charge!", LogType.Debuff);
    }

    private void WeakenedResolve(int teamId)
    {
        foreach (var enemy in GetEnemies(teamId))
        {
            var eff = new StatusEffect("Weakened Resolve", StatusEffectType.AccuracyModifier, 2, -0.2f, null, isDebuff: true);
            enemy.AddStatusEffect(eff);
        }
        Logger.Instance.PostLog("Weakened Resolve activated: Enemies -20% Accuracy!", LogType.Debuff);
    }

    // ---------------------- Helpers ----------------------

    private List<GameCharacter> GetAllies(int teamId)
    {
        if (BattleManager.Instance == null)
        {
            Debug.Log("BattleManager is null");
            return new List<GameCharacter>();
        }
        return BattleManager.Instance.GetTeam(teamId);
    }

    private List<GameCharacter> GetEnemies(int teamId)
    {
        if (BattleManager.Instance == null)
        {
            Debug.Log("BattleManager is null");
            return new List<GameCharacter>();
        }
        return BattleManager.Instance.GetTeam(teamId == 1 ? 2 : 1);
    }
}
