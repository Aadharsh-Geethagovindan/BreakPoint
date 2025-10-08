using UnityEngine;

using System.Linq;
public enum StatusEffectType
{
    None,
    DamageOverTime,
    HealingOverTime,
    Shield,
    AccuracyModifier,
    DamageModifier,
    Custom,
    ResistanceModifier,
    DodgeModifier,
    Stun,
    CDModifier,
    DurationModifier,
    SPDModifier,
    CritRateModifier,
    CritDMGModifier
}

public enum DurationTargetingMode
{
    All,
    AllBuffs,
    AllDebuffs,
    SingleBuff,
    SingleDebuff,
    SpecificType, // (optional) if you want to filter by StatusEffectType later
}


public class StatusEffect
{
    public string Name { get; private set; }
    public StatusEffectType Type { get; private set; }
    public int Duration { get; private set; }
    public float Value { get; private set; } // e.g., 10 damage per turn, or +0.2 damage multiplier
    public GameCharacter Source { get; private set; } // Who applied the effect
    public DamageType DamageType { get; private set; } = DamageType.True;
    public bool IsDebuff { get; private set; }
    public float ApplyChance;
    public int CooldownChangeAmount = 0;  // positive = increase CD, negative = reduce CD
    public AbilityType? AffectedAbilityType = null; // optional targeting (e.g., Skill only)
    public bool ToDisplay;
    public DurationTargetingMode DurationTargeting;


    public StatusEffect(string name, StatusEffectType type, int duration, float value, GameCharacter source, DamageType damageType = DamageType.True, bool isDebuff = false, float applyChance = 1f, bool toDisplay = true, DurationTargetingMode targetingMode = DurationTargetingMode.All)
    {
        Name = name;
        Type = type;
        Duration = duration;
        Value = value;
        Source = source;
        DamageType = damageType;
        IsDebuff = isDebuff;
        ApplyChance = applyChance;
        ToDisplay = toDisplay;
        DurationTargeting = targetingMode;
        //Debug.Log($"StatusEffect created: {name}, ToDisplay = {ToDisplay}");
    }

    public StatusEffect(StatusEffect other)
    {
        this.Name = other.Name;
        this.Type = other.Type;
        this.Duration = other.Duration;
        this.Value = other.Value;
        this.Source = other.Source;
        this.DamageType = other.DamageType;
        this.IsDebuff = other.IsDebuff;
        this.ApplyChance = other.ApplyChance;
        this.ToDisplay = other.ToDisplay;
        this.DurationTargeting = other.DurationTargeting;
        //Debug.Log($"COPY CONSTRUCTOR — {Name} ToDisplay: {ToDisplay}");

    }

    public static void ApplyTurnEffects(GameCharacter character)
    {
        //Debug.Log($"Applying start-of-turn status effects for {character.Name}");

        foreach (var effect in character.StatusEffects.ToList()) // Make a copy to allow removal during iteration
        {
            effect.ApplyEffect(character);
            //effect.TickDuration();

            if (effect.Duration <= 0)
            {
                character.RemoveStatusEffect(effect);
                //Debug.Log($"{effect.Name} expired on {character.Name}");
            }
        }
    }



    // Apply the effect to the target on their turn
    public void ApplyEffect(GameCharacter target)
    {
        int valueDone = 0;
        switch (Type)
        {
            case StatusEffectType.DamageOverTime:
            {
                    float dotValue = Value;

                 // if the caster has "Corrupt DoT Amp", boost DoT damage
                if (GameModeService.IsRevamped && Source != null) 
                {                                                 
                    float amp = 0f;                               
                    foreach (var se in Source.StatusEffects)      
                    {                                             
                        if (se.Type == StatusEffectType.Custom    
                            && se.Name == "Corrupt DoT Amp")      
                        {                                         
                            amp += se.Value;                       // stacks if multiple present
                        }                                         
                    }                                             
                    dotValue *= 1f + amp;                       
                }                                                 

                valueDone = target.TakeDamage(Mathf.RoundToInt(dotValue), DamageType);
                break;
            }
            case StatusEffectType.HealingOverTime:
                valueDone = target.Heal(Mathf.RoundToInt(Value));
                if (Source != null)
                {
                    Debug.Log($"{Source.Name} gained {valueDone} charge");
                    Source.IncreaseCharge(valueDone);
                }
                break;

            case StatusEffectType.Shield:
                target.AddShield(Mathf.RoundToInt(Value));
                break;

            case StatusEffectType.DurationModifier:

                {
                    bool ShouldAffect(StatusEffect eff)
                    {
                        switch (DurationTargeting)
                        {
                            case DurationTargetingMode.SingleBuff:
                            case DurationTargetingMode.AllBuffs:
                                return !eff.IsDebuff && eff.Type != StatusEffectType.None;
                            case DurationTargetingMode.SingleDebuff:
                            case DurationTargetingMode.AllDebuffs:
                                return eff.IsDebuff && eff.Type != StatusEffectType.None;
                            default:
                                return false;
                        }
                    }

                    var matches = target.StatusEffects.Where(ShouldAffect).ToList();

                    if (DurationTargeting == DurationTargetingMode.SingleBuff || DurationTargeting == DurationTargetingMode.SingleDebuff)
                    {
                        if (matches.Count > 0)
                            matches[0].Duration += Mathf.RoundToInt(Value);
                    }
                    else
                    {
                        foreach (var eff in matches)
                            eff.Duration += Mathf.RoundToInt(Value);
                    }

                    break;
                }

            // These are now handled dynamically in GameCharacter via GetModifiedX()
            case StatusEffectType.SPDModifier:
            case StatusEffectType.AccuracyModifier:
            case StatusEffectType.DamageModifier:
            case StatusEffectType.DodgeModifier:
            case StatusEffectType.ResistanceModifier:
                // No direct stat modification needed — effects are accounted for dynamically
                break;
        }
    }


    public void TickDuration()
    {
        Duration--;
    }

    public bool IsExpired()
    {
        return Duration <= 0;
    }

    public string GetIconName()
    {
        switch (Type)
        {
            case StatusEffectType.DamageOverTime:
                //Debug.Log($"{DamageType.ToString().ToLower()}");
                return $"{DamageType.ToString().ToLower()}";  // e.g., "dot_fire"
            case StatusEffectType.HealingOverTime:
                return "hot";
            case StatusEffectType.Stun:
                return "Stunned";
            case StatusEffectType.AccuracyModifier:
                return IsDebuff ? "accDebuff" : "accBuff";
            case StatusEffectType.DamageModifier:
                return IsDebuff ? "dmgDebuff" : "dmgBuff";
            case StatusEffectType.ResistanceModifier:
                return IsDebuff ? "resDebuff" : "resBuff";
            case StatusEffectType.DodgeModifier:
                return IsDebuff ? "dodgeDebuff" : "dodgeBuff";
            case StatusEffectType.CDModifier:
            case StatusEffectType.SPDModifier:
            case StatusEffectType.CritRateModifier:
            case StatusEffectType.CritDMGModifier:
                return "passiveIcon";
            default:
                Debug.Log("Could not match");
                return null;
                
        }
    }

}