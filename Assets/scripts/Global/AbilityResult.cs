using System.Collections.Generic;

[System.Serializable]
public class AbilityResult
{
    public int CasterId;
    public AbilityType AbilityType;

    public List<TargetResult> Targets = new List<TargetResult>();
    public bool EndsTurn;     // optional, useful later
}

[System.Serializable]
public class TargetResult
{
    public int TargetId;
    public bool Hit;          
    public bool Crit;         
    public int Damage;        // positive for damage, negative for healing
    public int HPAfter;

    public List<AppliedStatusEffectResult> AppliedEffects = new List<AppliedStatusEffectResult>();
}

[System.Serializable]
public class AppliedStatusEffectResult
{
    public string EffectType;
    public int Duration;
    public float Magnitude;
}
