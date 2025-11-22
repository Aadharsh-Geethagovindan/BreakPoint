using System.Collections.Generic;

public class UseAbilityCommand
{
    public GameCharacter Caster { get; }
    public Ability Ability { get; }
    public List<GameCharacter> Targets { get; }

    public UseAbilityCommand(GameCharacter caster, Ability ability, List<GameCharacter> targets)
    {
        Caster = caster;
        Ability = ability;
        // Defensive copy so later ClearTargeting() doesn’t mutate this
        Targets = targets != null ? new List<GameCharacter>(targets) : new List<GameCharacter>();
    }

    // Convert object-based command -> ID-based DTO
    public UseAbilityCommandData ToData()
    {
        var data = new UseAbilityCommandData
        {
            CasterId = Caster.Id,
            AbilityType = Ability.AbilityType
        };

        foreach (var target in Targets)
        {
            data.TargetIds.Add(target.Id);
        }

        return data;
    }

    // Convert ID-based DTO -> object-based command
    public static UseAbilityCommand FromData(UseAbilityCommandData data)
    {
        if (data == null)
            return null;

        var bm = BattleManager.Instance;
        if (bm == null)
            return null;

        var caster = bm.GetCharacterById(data.CasterId);
        if (caster == null)
        {
            UnityEngine.Debug.LogError($"UseAbilityCommand.FromData: no caster with Id {data.CasterId}");
            return null;
        }

        var ability = caster.GetAbilityOfType(data.AbilityType);
        if (ability == null)
        {
            UnityEngine.Debug.LogError($"UseAbilityCommand.FromData: caster {caster.Name} has no ability of type {data.AbilityType}");
            return null;
        }

        var targets = new List<GameCharacter>();
        foreach (var targetId in data.TargetIds)
        {
            var t = bm.GetCharacterById(targetId);
            if (t != null)
                targets.Add(t);
        }

        if (targets.Count == 0)
        {
            UnityEngine.Debug.LogWarning("UseAbilityCommand.FromData: no valid targets resolved.");
        }

        return new UseAbilityCommand(caster, ability, targets);
    }
}

public class SkipTurnCommand
{
    public GameCharacter Character { get; }

    public SkipTurnCommand(GameCharacter character)
    {
        Character = character;
    }

    public SkipTurnCommandData ToData()
    {
        return new SkipTurnCommandData
        {
            CharacterId = Character != null ? Character.Id : -1
        };
    }

    public static SkipTurnCommand FromData(SkipTurnCommandData data)
    {
        if (data == null)
            return null;

        var bm = BattleManager.Instance;
        if (bm == null)
            return null;

        GameCharacter character = null;
        if (data.CharacterId > 0)
        {
            character = bm.GetCharacterById(data.CharacterId);
        }

        return new SkipTurnCommand(character);
    }
}

public class UseAbilityCommandData
{
    public int CasterId;
    public AbilityType AbilityType;
    public List<int> TargetIds = new List<int>();
}
public class SkipTurnCommandData
{
    public int CharacterId;
}