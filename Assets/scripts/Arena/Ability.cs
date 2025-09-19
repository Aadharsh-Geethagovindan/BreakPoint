using System.Collections.Generic;
using UnityEngine;
using System;
using Breakpoint.Revamped;
public enum AbilityType
{
    Passive,
    Normal,
    Skill,
    Signature
}

public enum TargetType
{
    Self,
    Ally,
    AllyOrSelf,
    Enemy,
    All
}

public enum DamageType
{
    None,
    Arcane,
    Elemental,
    Force,
    Corrupt,
    True // Bypasses resistances
}

public class Ability
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    public AbilityType AbilityType { get; private set; }

    public int BaseCooldown { get; private set; }
    public int CurrentCooldown { get; set; }

    public int ChargeRequirement { get; private set; } // Only for Signature

    public int Damage { get; private set; }
    public int Healing { get; private set; }
    public int Shielding { get; private set; }
    public int MaxTargets { get; private set; }
    public List<StatusEffect> StatusEffectsApplied { get; private set; }

    public TargetType TargetType { get; private set; }
    public DamageType DamageType { get; private set; }
    public bool RequiresCustomLogic { get; set; } = false;
    public Func<GameCharacter, GameCharacter, int> CustomDamageOverride;
    int totalEffectValue = 0;


    public Ability(string name, string description, AbilityType type, int baseCooldown,
               int damage, int healing, int shielding, int chargeReq,
               TargetType targetType, int maxTargets, List<StatusEffect> statusEffectsApplied = null, DamageType damageType = DamageType.True)
    {
        Name = name;
        Description = description;
        AbilityType = type;
        BaseCooldown = baseCooldown;
        CurrentCooldown = 0;
        Damage = damage;
        Healing = healing;
        Shielding = shielding;
        ChargeRequirement = chargeReq;
        TargetType = targetType;
        MaxTargets = maxTargets;
        StatusEffectsApplied = statusEffectsApplied ?? new List<StatusEffect>();
        DamageType = damageType;
    }

    public bool IsUsable(int currentCharge)
    {
        bool cooldownReady = CurrentCooldown <= 0;
        bool hasCharge = currentCharge >= ChargeRequirement;
        return cooldownReady && hasCharge;
    }

    public void Apply(GameCharacter user, List<GameCharacter> targets, IReadOnlyDictionary<GameCharacter, bool> resolvedHits)
    {
        OutcomeFlags outcomeFlags = OutcomeFlags.None;
        //Debug.Log($"Applying {Name}");
        foreach (GameCharacter target in targets)
        {
            bool isEnemy = user.Enemies.Contains(target);

            // Decide hit using resolved results if available; otherwise fallback to old logic
            bool willHit = true;
            if (Damage > 0 && isEnemy)
            {
                if (resolvedHits != null && resolvedHits.TryGetValue(target, out bool pre))
                {
                    willHit = pre;
                }
                else
                {
                    // Fallback to legacy check (keeps old behavior if caller didn't resolve)
                    float hitChance = user.GetModifiedAccuracy() * (1f - target.GetModifiedDodgeChance());
                    float roll = UnityEngine.Random.value;
                    willHit = roll <= hitChance;
                }

                if (!willHit)
                {
                    // Timing is correct: Apply is called AFTER visuals
                    var missEvent = new GameEventData()
                        .Set("Source", user)
                        .Set("Target", target)
                        .Set("Ability", this);
                    EventManager.Trigger("OnMiss", missEvent);
                    continue; // skip damage/effects on this target
                }
            }

            int baseDamage = GetEffectiveBaseDamage(user, target);
            // Apply Damage
            if (baseDamage > 0)
            {
                int dmg = Mathf.RoundToInt(baseDamage * user.GetModifiedDamageMultiplier()); // get damage without crit

                //check for crit, if crit, modify dmg
                float roll = UnityEngine.Random.value; // 0–1
                bool isCrit = roll < user.CritRate;
                if (isCrit)
                {
                    dmg = Mathf.CeilToInt(dmg * user.CritDMG);

                    EventManager.Trigger("OnCriticalHit", new GameEventData()
                    .Set("Source", user)
                    .Set("Target", target)
                    .Set("CritRate", user.CritRate)
                    .Set("CritDamage", user.CritDMG));
                }


                dmg = target.TakeDamage(dmg, DamageType);
                //Debug.Log($"{target.Name} took {dmg} damage");
                EventManager.Trigger("OnDamageDealt", new GameEventData()
                                .Set("Source", user)
                                .Set("Target", target)
                                .Set("Amount", dmg)
                                .Set("Type", DamageType)
                            );




                totalEffectValue += dmg;
            }

            // Apply Healing
            if (Healing > 0)
            {
                target.Heal(Healing);
                totalEffectValue += Healing;

                EventManager.Trigger("OnHealApplied", new GameEventData()
                    .Set("Source", user)
                    .Set("Target", target)
                    .Set("Amount", Healing)
                );

                outcomeFlags |= OutcomeFlags.Heal; 
            }

            // Apply Shield
            if (Shielding > 0)
            {
                target.AddShield(Shielding);
                totalEffectValue += Shielding;

                EventManager.Trigger("OnShieldApplied", new GameEventData()
                    .Set("Source", user)
                    .Set("Target", target)
                    .Set("Amount", Shielding)
                );
                outcomeFlags |= OutcomeFlags.Shield; 
            }
            // Apply Status Effects
            foreach (var effect in StatusEffectsApplied)
            {
                float roll = UnityEngine.Random.value;
                if (roll <= effect.ApplyChance)
                {
                    var applied = new StatusEffect(effect.Name, effect.Type, effect.Duration, effect.Value, user, effect.DamageType, effect.IsDebuff, effect.ApplyChance, toDisplay: effect.ToDisplay, targetingMode: effect.DurationTargeting)
                    { AffectedAbilityType = effect.AffectedAbilityType, CooldownChangeAmount = effect.CooldownChangeAmount };
                    target.AddStatusEffect(applied);
                    //Debug.Log($"{effect.Name} applied to {target.Name} with chance {effect.ApplyChance}");
                    EventManager.Trigger("OnStatusApplied", new GameEventData()
                            .Set("Source", user)
                            .Set("Target", target)
                            .Set("Effect", applied)
                        );
                        
                    //Set Status effect flags
                    switch (applied.Type)
                    {
                        case StatusEffectType.Stun:
                            outcomeFlags |= OutcomeFlags.Stun;
                            break;
                        case StatusEffectType.DamageOverTime:
                            outcomeFlags |= OutcomeFlags.Dot;
                            break;
                    }
                }
                else
                {
                    Debug.Log($"{effect.Name} failed to apply to {target.Name} (rolled {roll})");
                    Logger.Instance.PostLog($"{effect.Name} failed to apply to {target.Name} (rolled {roll})", LogType.Miss);
                }

                // Trigger BUFF popup if it's not a debuff
                if (!effect.IsDebuff && (effect.Type == StatusEffectType.DamageModifier ||
                                        effect.Type == StatusEffectType.AccuracyModifier ||
                                        effect.Type == StatusEffectType.ResistanceModifier ||
                                        effect.Type == StatusEffectType.Shield))
                {
                    var buffEvt = new GameEventData();
                    buffEvt.Set("Target", target); // or "Source", if that's more appropriate
                    buffEvt.Set("Effect", effect);
                    EventManager.Trigger("OnBuffApplied", buffEvt);

                }
            }
        }

        // Set cooldown and charge
        CurrentCooldown = BaseCooldown;

        if (AbilityType == AbilityType.Signature)
            user.ResetCharge();

        //  Charge gain based on positive effect contribution
        if (AbilityType != AbilityType.Signature)
        {
            user.IncreaseCharge(totalEffectValue);
            totalEffectValue = 0; // reset the effect values 
        }

        user.HasActedThisTurn = true;

       EventManager.Trigger("OnAbilityResolved", new GameEventData()
                            .Set("Source", user)                
                            .Set("Targets", targets)            
                            .Set("Ability", this)               
                            .Set("Essence", this.DamageType)    
                            .Set("Outcome", outcomeFlags)       
                            .Set("TeamId", user.TeamId)         
                            );
    }

    public void Apply(GameCharacter user, List<GameCharacter> targets)
    {
        Apply(user, targets, resolvedHits: null);
    }

    public List<HitResolution> ResolveHits(GameCharacter user, List<GameCharacter> targets)
    {
        var results = new List<HitResolution>(targets.Count);

        foreach (var target in targets)
        {
            bool willHit = true;

            // Only roll when it’s a damaging offensive action
            bool isEnemy = user.Enemies.Contains(target);
            if (Damage > 0 && isEnemy)
            {
                target.SetHasBeenAttackedThisTurn(true);

                float hitChance = user.GetModifiedAccuracy() * (1f - target.GetModifiedDodgeChance());
                float roll = UnityEngine.Random.value;
                willHit = (roll <= hitChance);
            }

            results.Add(new HitResolution(target, willHit));
        }

        return results;
    }

    public void SetDamage(int amount)
    {
        Damage = amount;
    }

    public void SetHealing(int amount)
    {
        Healing = amount;
    }

    public void SetShielding(int amount)
    {
        Shielding = amount;
    }
    public int GetEffectiveBaseDamage(GameCharacter user, GameCharacter target)
    {
        return CustomDamageOverride != null ? CustomDamageOverride(user, target) : Damage;
    }
    public void IncreaseCooldown(int amount)
    {
        CurrentCooldown += amount;
    }

    public void ReduceCooldown(int amount)
    {
        CurrentCooldown = Mathf.Max(CurrentCooldown - amount, 0);
    }
}
public struct HitResolution
{
    public GameCharacter Target;
    public bool WillHit;

    public HitResolution(GameCharacter target, bool willHit)
    {
        Target = target;
        WillHit = willHit;
    }
}