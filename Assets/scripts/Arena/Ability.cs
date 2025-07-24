using System.Collections.Generic;
using UnityEngine;
using System;
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
    Physical,
    Fire,
    Water,
    Ice,
    Air,
    Earth,
    Lightning,
    Energy,
    Poison,
    Psychic,
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

    public void Apply(GameCharacter user, List<GameCharacter> targets)
    {
        foreach (GameCharacter target in targets)
        {
            bool isEnemy = user.Enemies.Contains(target);

            // Hit check only if attacking an enemy with damage
            if (Damage > 0 && isEnemy)
            {
                target.SetHasBeenAttackedThisTurn(true);
                float hitChance = user.GetModifiedAccuracy() * (1f - target.GetModifiedDodgeChance());
                float roll = UnityEngine.Random.value;

                if (roll > hitChance)
                {
                    Debug.Log($"{user.Name}'s {Name} missed {target.Name}!");
                    Logger.Instance.PostLog($"{user.Name}'s {Name} missed {target.Name}!", LogType.Miss);
                    SoundManager.Instance.PlaySFX("miss"); // play sound
                    PopupManager.Instance.ShowPopup(PopupType.Miss); // show visual MISS effect
                    continue; // Skip to next target
                }
            }

            int baseDamage = GetEffectiveBaseDamage(user, target);
            // Apply Damage
            if (baseDamage > 0)
            {
                int dmg = Mathf.RoundToInt(baseDamage * user.GetModifiedDamageMultiplier());
                dmg = target.TakeDamage(dmg, DamageType);

                EventManager.Trigger("OnDamageDealt", new GameEventData()
                                .Set("Source", user)
                                .Set("Target", target)
                                .Set("Amount", dmg)
                                .Set("Type", DamageType)
                            );

                SoundManager.Instance.PlaySFX("hit");
                PopupManager.Instance.ShowPopup(PopupType.Hit);
                ActiveCharPanel panel = UnityEngine.Object.FindFirstObjectByType<ActiveCharPanel>();
                CharacterCardUI card = panel?.FindCardForCharacter(target);
                if (card != null)
                {
                    card.Shake(); // 🌀 Trigger the shake animation
                }
                totalEffectValue += dmg;
            }

            // Apply Healing
            if (Healing > 0)
            {
                target.Heal(Healing);
                totalEffectValue += Healing;

                EventManager.Trigger("OnHealed", new GameEventData()
                    .Set("Source", user)
                    .Set("Target", target)
                    .Set("Amount", Healing)
                );
            }

            // Apply Shield
            if (Shielding > 0)
            {
                target.AddShield(Shielding);
                totalEffectValue += Shielding;

                EventManager.Trigger("OnShielded", new GameEventData()
                    .Set("Source", user)
                    .Set("Target", target)
                    .Set("Amount", Shielding)
                );
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
                    SoundManager.Instance.PlaySFX("buff"); // play sound
                    PopupManager.Instance.ShowPopup(PopupType.Buff);
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

       // Debug.Log($"{user.Name} used {Name} on {targets.Count} target(s).");
        //Logger.Instance.PostLog($"{user.Name} used {Name} on {targets.Count} target(s).", LogType.Info);
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
