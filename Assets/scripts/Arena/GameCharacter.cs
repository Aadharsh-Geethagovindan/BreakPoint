using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class GameCharacter
{

    [Header("Base Stats")]
    public string Name { get; private set; }
    public int HP { get; private set; }
    public int MaxHP { get; private set; }
    public int Shield { get; private set; } = 0;
    public int Speed { get; private set; }
    public int Charge { get; private set; }
    public int SigChargeReq { get; private set; }
    public string Lore { get; private set; }
    public string Species { get; private set; }


    [Header("Game Specific Stats")]
    public float DamageMultiplier { get; private set; } = 1f;
    public string Affil { get; private set; }
    public float Accuracy { get; private set; } = 1f; // 1 = 100%
    public float DodgeChance { get; private set; } = 0f; // 0 = 0%
    private int totalHealed = 0;
    private int healThresholdsMet = 0;
    public int TeamId { get; private set; }
    public Dictionary<DamageType, float> Resistances { get; private set; }
    public float CritRate { get; private set; } = 0.1f;     // default 5%
    public float CritDMG { get; private set; } = 1.5f;  // default 150% damage


    [Header("Bool Trackers")]
    public int LastDamageTaken = 0;
    private bool hasBeenAttackedThisTurn = false;
    public bool HasActedThisTurn = false;
    public bool IsDead = false;
    public bool HasUsedOneTimePassive { get; private set; } = false;
    public List<StatusEffect> StatusEffects { get; private set; }

    [Header("Abilities")]
    public Ability NormalAbility { get; private set; }
    public Ability SkillAbility { get; private set; }
    public Ability SignatureAbility { get; private set; }
    public Ability PassiveAbility { get; private set; }

    
    [Header("Misc.")]
    private List<GameCharacter> allies = new List<GameCharacter>();
    private List<GameCharacter> enemies = new List<GameCharacter>();

    public string ImageName { get; private set; }




    public GameCharacter(string name, int hp, int speed, int sigChargeReq,
                         Ability normal, Ability skill, Ability signature, Ability passive, string imageName, string affil, string lore, string species)
    {
        Name = name;
        MaxHP = hp;
        HP = hp;
        Speed = speed;
        SigChargeReq = sigChargeReq;
        Affil = affil;
        Lore = lore;
        Species = species;

        Charge = 0;
        DamageMultiplier = 1f;
        Accuracy = 1f;
        DodgeChance = 0f;
        StatusEffects = new List<StatusEffect>();

        NormalAbility = normal;
        SkillAbility = skill;
        SignatureAbility = signature;
        PassiveAbility = passive;
        ImageName = imageName;

        Resistances = new Dictionary<DamageType, float>(); // all default resistances are 0%
        foreach (DamageType type in System.Enum.GetValues(typeof(DamageType)))
        {
            Resistances[type] = 0f;
        }
    }


    //**********************************************DAMAGE APPLICATION/MITIGATION *********************************************************************
    //**********************************************DAMAGE APPLICATION/MITIGATION *********************************************************************
    //**********************************************DAMAGE APPLICATION/MITIGATION *********************************************************************
    //**********************************************DAMAGE APPLICATION/MITIGATION *********************************************************************
    public int TakeDamage(int amount, DamageType type)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"[TakeDamage] {Name} took no damage of type {type} — potential bug?");
            Logger.Instance.PostLog($"[TakeDamage] {Name} took no damage of type {type} — potential bug?", LogType.Warning);
        }
        float resistance = GetModifiedResistance(type);
        float modifiedAmount = type == DamageType.True ? amount : amount * (1 - resistance);
        int finalDamage = Mathf.RoundToInt(modifiedAmount);
        int totDamage = finalDamage;
        if (Shield > 0)
        {
            int absorbed = Mathf.Min(Shield, finalDamage);
            Shield -= absorbed;
            finalDamage -= absorbed;
        }

        //character specific trigger
        // 👇 Jack's passive — only once per turn
        if (Name == "Jack" && HP > 1 && finalDamage >= HP && !HasUsedOneTimePassive)
        {
            finalDamage = HP - 1;
            MarkOneTimePassive();
            Debug.Log("Jack's passive triggered! Survived fatal damage.");
        }

        if (finalDamage > 0)
        {

            HP -= finalDamage;
            if (HP < 0) HP = 0;
            //Debug.Log($"{Name} took {finalDamage} {type} damage. HP: {HP}/{MaxHP}");
            //Logger.Instance.PostLog($"{Name} took {finalDamage} {type} damage. HP: {HP}/{MaxHP}", LogType.Damage);
        }
        SetHasBeenAttackedThisTurn(true);
        LastDamageTaken = totDamage;
        return totDamage;
    }

    public void AddShield(int amount)
    {
        var evt = new GameEventData(); evt.Set("Target", this); evt.Set("Amount", amount);
        EventManager.Trigger("OnShielded", evt);

        Shield += amount;

    }

    public int Heal(int amount)
    {

        var evt = new GameEventData(); evt.Set("Target", this); evt.Set("Amount", amount);
        EventManager.Trigger("OnHealed", evt);

        //******************************************************************************************************
        //Character specific conditions
        if (Name == "Faru")
        {
            amount *= 2;
        }
        if (Name == "Huron")
        {
            int previousHP = HP;
            HP += amount;
            if (HP > MaxHP) HP = MaxHP;

            int actualHealed = HP - previousHP;
            totalHealed += actualHealed;

            while (healThresholdsMet < 3 && totalHealed >= (healThresholdsMet + 1) * 70)
            {
                healThresholdsMet++;
                ModifyDamageMultiplier(0.25f);
                Logger.Instance.PostLog($"Huron passive triggered: +25% damage (x{healThresholdsMet})", LogType.Passive);
            }

            return actualHealed;
        }
        //******************************************************************************************************

        //actual heal
        HP += amount;
        if (HP > MaxHP) HP = MaxHP;
        return amount;

    }

    //*******************************************************************************************************************
    //************************************MODIFIERS**********************************************************************
    //************************************MODIFIERS**********************************************************************
    //************************************MODIFIERS**********************************************************************
    public void SetMaxHP(int value)
    {
        MaxHP = value;
    }
    public void SetHP(int value)
    {
        HP = value;
    }

    public void AddCritRate(float value)
    {
        CritRate += value;
    }

    public void AddCritDMG(float value)
    {
        CritDMG += value;
    }
    public void ModifyAccuracy(float amount)
    {
        Accuracy += amount;
        Accuracy = Mathf.Clamp(Accuracy, 0f, 2f); // Optional clamp
        Debug.Log($"{Name}'s accuracy modified by {amount}. New accuracy: {Accuracy}");
        Logger.Instance.PostLog($"{Name}'s accuracy modified by {amount}. New accuracy: {Accuracy}", LogType.Info);
    }

    public void ModifyDamageMultiplier(float amount)
    {
        DamageMultiplier += amount;
        DamageMultiplier = Mathf.Clamp(DamageMultiplier, 0f, 5f); // Optional clamp
        Debug.Log($"{Name}'s damage multiplier modified by {amount}. New multiplier: {DamageMultiplier}");
        Logger.Instance.PostLog($"{Name}'s damage multiplier modified by {amount}. New multiplier: {DamageMultiplier}", LogType.Info);
    }

    public void ModifyResistance(DamageType type, float value)
    {
        if (Resistances.ContainsKey(type))
            Resistances[type] += value;
        else
            Resistances[type] = value;
    }

    public void ModifyDodge(float value)
    {
        DodgeChance += value;
        DodgeChance = Mathf.Clamp(DodgeChance, 0f, 1f);
    }

    public float GetModifiedDamageMultiplier()
    {
        float baseMultiplier = DamageMultiplier;
        foreach (var effect in StatusEffects)
        {
            if (effect.Type == StatusEffectType.DamageModifier)
                baseMultiplier += effect.Value;
        }
        return Mathf.Clamp(baseMultiplier, 0f, 5f);
    }

    public float GetModifiedCritRate()
    {
        float r = CritRate; // base
        foreach (var se in StatusEffects)
            if (se.Type == StatusEffectType.CritRateModifier)
                r += se.Value; // additive
        return Mathf.Clamp01(r);
    }

    // NEW
    public float GetModifiedCritDMG()
    {
        float m = CritDMG; // base (e.g., 1.5 = 150%)
        foreach (var se in StatusEffects)
            if (se.Type == StatusEffectType.CritDMGModifier)
                m += se.Value; // additive to multiplier
        return Mathf.Max(1f, m);
    }

    public int GetModifiedSpeed()
    {
        float mult = 1f; // 1.0 = no change
        foreach (var effect in StatusEffects)
        {
            if (effect.Type == StatusEffectType.SPDModifier)
                mult += effect.Value; // e.g., -0.25f makes mult 0.75
        }
        int result = Mathf.Max(1, Mathf.RoundToInt(Speed * mult));
        return result;
    }

    public float GetModifiedAccuracy()
    {
        float baseAccuracy = Accuracy;
        foreach (var effect in StatusEffects)
        {
            if (effect.Type == StatusEffectType.AccuracyModifier)
                baseAccuracy += effect.Value;
        }
        return Mathf.Clamp(baseAccuracy, 0f, 2f);
    }

    public float GetModifiedResistance(DamageType type)
    {
        float baseResistance = Resistances.ContainsKey(type) ? Resistances[type] : 0f;
        foreach (var effect in StatusEffects)
        {
            if (effect.Type == StatusEffectType.ResistanceModifier && effect.DamageType == type)
                baseResistance += effect.Value;
        }
        return Mathf.Clamp(baseResistance, -1f, 1f);
    }

    public float GetModifiedDodgeChance()
    {
        float totalModifier = DodgeChance;
        foreach (var effect in StatusEffects)
        {
            if (effect.Type == StatusEffectType.DodgeModifier)
                totalModifier += effect.Value;
        }

        return Mathf.Clamp(DodgeChance + totalModifier, 0f, 1f);
    }
    //*********************************************************************************************************************************************
    //*********************************************************************************************************************************************


    public int TotalHealed => totalHealed;
    public int HealThresholdsMet => healThresholdsMet;
    public void AddStatusEffect(StatusEffect effect)
    {
        if (Name == "Faru")
        {
            if (effect.Type == StatusEffectType.HealingOverTime || effect.Type == StatusEffectType.DamageModifier)
            {
                effect = new StatusEffect(
                    effect.Name,
                    effect.Type,
                    effect.Duration,
                    effect.Value * 2,
                    effect.Source,
                    effect.DamageType,
                    effect.IsDebuff
                );
            }
        }



        StatusEffects.Add(effect);
        EventManager.Trigger("OnStatusEffectApplied", new GameEventData()
            .Set("Target", this)
            .Set("Effect", effect)
        );
    }

    public void deathStatus(bool hasDied)
    {
        IsDead = hasDied;
    }
    public void MarkOneTimePassive()
    {
        HasUsedOneTimePassive = true;
    }
    public void RemoveStatusEffect(StatusEffect effect)
    {
        StatusEffects.Remove(effect);
    }

    public void IncreaseCharge(int amount)
    {
        Charge += amount;
        //Debug.Log($"{Name} gained {amount} charge. Current: {Charge}/{SignatureAbility.ChargeRequirement}");
        EventManager.Trigger("OnChargeIncreased", new GameEventData()
            .Set("Character", this)
            .Set("Amount", amount)
            );
    }

    public void ReduceCharge(int amount)
    {
        Charge -= amount;
        Charge = Mathf.Clamp(Charge, 0,10000);
        Debug.Log($"{Name} lost {amount} charge. Current: {Charge}/{SignatureAbility.ChargeRequirement}");
        EventManager.Trigger("OnChargeDecreased", new GameEventData()
                .Set("Character", this)
                .Set("Amount", amount)
            );
    }
    public void ResetCharge()
    {
        Charge = 0;
    }

    public bool CanUseSignature()
    {
        return Charge >= SigChargeReq;
    }

    public bool HasStatusEffect(StatusEffectType type)
    {
        return StatusEffects.Any(e => e.Type == type);
    }

    public bool HasBeenAttackedThisTurn => hasBeenAttackedThisTurn;

    public void SetHasBeenAttackedThisTurn(bool value)
    {
        hasBeenAttackedThisTurn = value;
    }

    public void ResetRoundFlags()
    {
        hasBeenAttackedThisTurn = false;
    }
    public void AddAlly(GameCharacter ally) => allies.Add(ally);
    public void AddEnemy(GameCharacter enemy) => enemies.Add(enemy);


    public Ability GetAbilityOfType(AbilityType type)
    {
        return type switch
        {
            AbilityType.Normal => NormalAbility,
            AbilityType.Skill => SkillAbility,
            AbilityType.Signature => SignatureAbility,
            AbilityType.Passive => null, // Not targetable by effects
            _ => null
        };
    }

    public List<GameCharacter> Allies => allies;
    public List<GameCharacter> Enemies => enemies;
    public void ClearAllies() => allies.Clear();
    public void ClearEnemies() => enemies.Clear();
    public void SetTeam(int teamId) => TeamId = teamId; 

    
}