using System.Collections.Generic;

[System.Serializable]
public class CharacterState
{
    public int Id;
    public string Name;
    public int TeamId;

    public int HP;
    public int MaxHP;

    public int SigCharge;
    public int SigChargeRequired;

    public bool IsDead;
    public bool IsStunned;

    public List<StatusEffectState> StatusEffects = new List<StatusEffectState>();
    public List<AbilityState> Abilities = new List<AbilityState>();
}


// NOTE:  ADD isDebuff HERE
[System.Serializable]
public class StatusEffectState
{
    public string Type;           // e.g. "Dot", "Hot", "Stun", "DamageMod"
    public string SourceName;     // Optional: who applied it
    public int RemainingTurns;
    public float Magnitude;       // Optional: depends on effect type
}

[System.Serializable]
public class AbilityState
{
    public string Name;
    public string AbilityType;    // "Normal", "Skill", "Signature", "Passive"
    public int CurrentCooldown;
    public int BaseCooldown;
    public bool IsUsable;         // convenience flag
}

[System.Serializable]
public class GameStateSnapshot
{
    public int RoundNumber;
    public int CurrentCharacterId;
    public int CurrentTeamId;

    public List<CharacterState> Characters = new List<CharacterState>();

    // If you want breakpoint info now:
    public float Player1BreakpointValue;
    public float Player2BreakpointValue;
    public List<int> TurnOrderIds = new();
}
