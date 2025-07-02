using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public static class CharacterFactory
{
    public static GameCharacter CreateCharacterByName(string name, CharacterDataArray statsSource)
    {
        CharacterData data = GetCharacterData(name, statsSource);
        if (data == null) return null;

        switch (name)
        {
            case "Arkhe":
                return CreateArkhe(data);
            case "Avarice":
                return CreateAvarice(data);
            case "Bessil":
                return CreateBessil(data);
            case "Breach Specialist":
                return CreateBreachSpecialist(data);
            case "Captain Dinso":
                return CreateCap(data);
            case "Constellian Trooper":
                return CreateConstellianTrooper(data);
            case "Faru":
                return CreateFaru(data);
            case "Huron":
                return CreateHuron(data);
            case "Jack":
                return CreateJack(data);
            case "KAS":
                return CreateKAS(data);
            case "Krakoa":
                return CreateKrakoa(data);
            case "Legionary":
                return CreateLegionary(data);
            case "Mizca":
                return CreateMizca(data);
            case "Nou":
                return CreateNou(data);
            case "Olthar":
                return CreateOlthar(data);
            case "Raish":
                return CreateRaish(data);
            case "Rei":
                return CreateRei(data);
            case "Sedra":
                return CreateSedra(data);
            case "Ulmika":
                return CreateUlmika(data);
            case "VyGar":
                return CreateVyGar(data);
            case "Rover":
                return CreateRover(data);
            case "Temple Guard":
                return CreateTempleGuard(data);
            case "TRex":
                return CreateTRex(data);
            case "Skirvex":
                return CreateSkirvex(data);
            case "Sanguine":
                return CreateSanguine(data);
            case "Rellin":
                return CreateRellin(data);
            case "Virae":
                return CreateVirae(data);
            case "Vas Drel":
                return CreateVasDrel(data);
            case "Vemk Parlas":
                return CreateVemk(data);
            case "Trustless Engineer":
                return CreateTrustless(data);
            default:
                Debug.LogWarning($"No factory method defined for character: {name}");
                return null;
        }
    }


    private static CharacterData GetCharacterData(string name, CharacterDataArray statsSource)
    {
        foreach (var character in statsSource.characters)
        {
            if (character.name == name)
                return character;
        }
        Debug.LogWarning($"Stats for {name} not found in CharacterDataArray.");
        return null;
    }

    private static GameCharacter CreateArkhe(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Self, 1);

        var airburstEffects = new List<StatusEffect>
        {
            new StatusEffect("Stunned", StatusEffectType.Stun, 1, 0, null, isDebuff: true, applyChance: 0.15f)
        };

        var normal = new Ability(moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
                                30, 0, 0, 0, TargetType.Enemy, 1, airburstEffects, DamageType.Air);

        
        var skill = new Ability(moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
                                0, 30, 15, 0, TargetType.AllyOrSelf, 1);

        var sig = new Ability(moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
                            100, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, null, DamageType.Fire);

        var arkhe = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive,data.imageName, data.affiliation, data.lore, data.species);

        // Optional: Give arkhe moderate air resistance
        arkhe.Resistances[DamageType.Air] = 0.2f;

        return arkhe;
    }

    private static GameCharacter CreateAvarice(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Self, 1);

        var normal = new Ability(moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
                                30, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Water);

        var skill = new Ability(moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
                                0, 75, 0, 0, TargetType.AllyOrSelf, 1);

        var sig = new Ability(moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
                            0, 0, 0, data.SigChargeReq, TargetType.Ally, 1);

        var Avarice = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);

        // Optional: Avarice is resistant to Water and Ice, weak to Fire
        Avarice.Resistances[DamageType.Water] = 0.3f;
        Avarice.Resistances[DamageType.Ice] = 0.3f;
        Avarice.Resistances[DamageType.Fire] = -0.2f;

        return Avarice;
    }
    private static GameCharacter CreateBessil(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Self, 1);
        // Passive handled in PassiveManager: reduce all enemy accuracy by 15% at game start

        var normal = new Ability(moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown, 40, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Psychic);

        var skill = new Ability(moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown, 20, 0, 0, 0, TargetType.Enemy, 2, null, DamageType.Psychic);
        skill.CustomDamageOverride = (user, target) =>
        {
            int debuffCount = target.StatusEffects.Count(e => e.IsDebuff && e.ToDisplay);
            return 30 + (10 * debuffCount);
        };

        var stun = new StatusEffect("Stun", StatusEffectType.Stun, 1, 0, null, isDebuff: true, applyChance: 0.5f);
        var sig = new Ability(moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown, 80, 0, 0, data.SigChargeReq, TargetType.Enemy, 2, new List<StatusEffect> { stun }, DamageType.Psychic);

        GameCharacter bessil = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
        return bessil;
    }

    private static GameCharacter CreateBreachSpecialist(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        // Passive: fire resistance to all allies
        var fireRes = new StatusEffect("Insulated Lining", StatusEffectType.ResistanceModifier, 5, 0.15f, null, DamageType.Fire, isDebuff: false);
        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.AllyOrSelf, 3, new List<StatusEffect> { fireRes });

        // Normal
        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            18, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Fire
        );

        // Skill
        var accuracyDebuff = new StatusEffect("Blinded", StatusEffectType.AccuracyModifier, 1, -0.10f, null, isDebuff: true);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            10, 0, 0, 0, TargetType.Enemy, 2, new List<StatusEffect> { accuracyDebuff }, DamageType.Fire
        );

        // Signature
        var burn = new StatusEffect("Burn", StatusEffectType.DamageOverTime, 1, 20, null, DamageType.Fire, isDebuff: true);
        var afterburn = new StatusEffect("Afterburn", StatusEffectType.ResistanceModifier, 3, -0.20f, null, DamageType.Fire, isDebuff: true, applyChance: 0.30f);
        var sigEffects = new List<StatusEffect> { burn, afterburn };
        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            10, 0, 0, data.SigChargeReq, TargetType.Enemy, 3, sigEffects, DamageType.Fire
        );

        GameCharacter breacher = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive,
                                                    data.imageName, data.affiliation, data.lore, data.species);
        return breacher;
    }
    private static GameCharacter CreateCap(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Self, 1);
        // Special case: Reflect energy attacks < 40 — tracked for PassiveManager or BattleManager

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            20, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Physical
        );

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            15, 0, 0, 0, TargetType.Enemy, 2, null, DamageType.Physical
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            0, 0, 0, data.SigChargeReq, TargetType.Enemy, 3
        );
        // Special case: Sig applies charge and clears debuffs — handled in BattleManager

        GameCharacter cap = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
        return cap;
    }
    private static GameCharacter CreateConstellianTrooper(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description,AbilityType.Passive, 0, 0, 0, 0, 0,TargetType.Self, 1);
        // Passive logic handled in PassiveManager: Bonus damage scaling with CAF-affiliated allies

        var normal = new Ability(moves[1].name, moves[1].description,AbilityType.Normal, moves[1].cooldown,20, 0, 0, 0, TargetType.Enemy, 1,null, DamageType.Energy);

        var skill = new Ability(moves[2].name, moves[2].description,AbilityType.Skill, moves[2].cooldown,35, 0, 0, 0, TargetType.Enemy, 1,null, DamageType.Energy);

        var sig = new Ability(moves[3].name, moves[3].description,AbilityType.Signature, moves[3].cooldown,22, 0, 0, data.SigChargeReq, TargetType.Enemy, 3,null, DamageType.Energy);

        GameCharacter trooper = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq,normal, skill, sig, passive,data.imageName, data.affiliation, data.lore, data.species);

        return trooper;
    }
    private static GameCharacter CreateFaru(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Self, 1);
        // Passive logic handled in PassiveManager: Double healing and damage buffs

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            25, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Physical
        );

        var burnEffect = new StatusEffect("Burn", StatusEffectType.DamageOverTime, 1, 20, null, DamageType.Fire,isDebuff: true,applyChance: .60f);
        var fireSwingEffects = new List<StatusEffect> { burnEffect };

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            45, 0, 0, 0, TargetType.Enemy, 1, fireSwingEffects, DamageType.Fire
        );
        // 60% chance to apply Burn — note for BattleManager handling

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            60, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, null, DamageType.Physical
        );

        // Custom damage override: +30 per active non-debuff effect
        sig.CustomDamageOverride = (user, target) =>
        {
            int buffCount = user.StatusEffects.Count(e => !e.IsDebuff && e.ToDisplay);
            return 60 + (30 * buffCount);
        };

        GameCharacter Faru = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
        return Faru;
    }

    private static GameCharacter CreateHuron(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Self, 1);
        // Special case: Gains +25% damage per 70 HP healed, up to 3 times (handled in PassiveManager)

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            50, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Energy
        );

        var solarFlareEffects = new List<StatusEffect>
        {
            new StatusEffect("Blinded", StatusEffectType.AccuracyModifier, 1, -0.2f, null,isDebuff: true)
        };
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            18, 0, 0, 0, TargetType.Enemy, 3, solarFlareEffects, DamageType.Energy
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            160, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, null, DamageType.Energy
        );

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }

    private static GameCharacter CreateJack(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Self, 1);
        // Special case: Prevents death once by reducing HP to 1 — to be handled in BattleManager or TakeDamage

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            20, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Energy
        );

        var mockedEffect = new List<StatusEffect>
        {
            new StatusEffect("Mocked", StatusEffectType.DodgeModifier, 1, -0.2f, null,isDebuff: true)
        };

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            0, 0, 0, 0, TargetType.Enemy, 1, mockedEffect
        );

        var psychEffects = new List<StatusEffect>
        {
            new StatusEffect("Motivated", StatusEffectType.DamageModifier, 1, 0.3f, null)
        };

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            0, 0, 10, data.SigChargeReq, TargetType.AllyOrSelf, 2, psychEffects
        );

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation,data.lore,data.species);
    }

        private static GameCharacter CreateKAS(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        // Passive: Calm Focus — Shield if not attacked
        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Self, 1);
        // Passive logic handled in PassiveManager based on HasBeenAttackedThisTurn

        // Normal: Starburst Stream
        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            40, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Physical
        );

        // Skill: Dual Wield Precision (2 targets)
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            25, 0, 0, 0, TargetType.Enemy, 2, null, DamageType.Physical
        );

        // Signature: Sword Skill: Eclipse (100 dmg + 10 shield to self)
        var sigEffects = new List<StatusEffect>
        {
            new StatusEffect("Dazed", StatusEffectType.AccuracyModifier, 1, -0.10f, null,isDebuff: true)
        };

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            100, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, sigEffects, DamageType.Energy
        );

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }

    private static GameCharacter CreateKrakoa(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Self, 1);
        // Special-case: PassiveManager will adjust fire + physical resistance based on missing HP

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            35, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Ice
        );

        var burnEffect = new StatusEffect("Chilled", StatusEffectType.DamageOverTime, 2, 20, null, DamageType.Ice, isDebuff: true);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            30, 0, 0, 0, TargetType.Enemy, 1, new List<StatusEffect> { burnEffect }, DamageType.Ice
        );

        var rageEffect = new StatusEffect("Enraged", StatusEffectType.DamageModifier, 2, 1f, null);
        var lockSkill = new StatusEffect("Enraged Lockout", StatusEffectType.CDModifier, 2, 1, null, toDisplay: false) {
            AffectedAbilityType = AbilityType.Skill,
            CooldownChangeAmount = 1
        };
        //Debug.Log($"[Factory] AffectedAbilityType: {lockSkill.AffectedAbilityType}, HasValue: {lockSkill.AffectedAbilityType.HasValue}");
        var healAndRage = new List<StatusEffect> { rageEffect, lockSkill };

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            0, 100, 0, data.SigChargeReq, TargetType.Self, 1, healAndRage
        );
        // Signature: heals 100, and enrages Krakoa (BattleManager can restrict actions while enraged)

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }

    private static GameCharacter CreateLegionary(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description,AbilityType.Passive, 0, 0, 0, 0, 0,TargetType.Self, 1);
        // Passive will require BattleManager hook: trigger shield once when HP drops below 30%

        var normal = new Ability(moves[1].name, moves[1].description,AbilityType.Normal, moves[1].cooldown,16, 0, 0, 0, TargetType.Enemy, 1,null, DamageType.Earth);

        var rupture = new StatusEffect("Rupture", StatusEffectType.DamageOverTime, 1, 20, null, DamageType.Earth, isDebuff: true, applyChance: 0.5f);
        var skill = new Ability(moves[2].name, moves[2].description,AbilityType.Skill, moves[2].cooldown,20, 0, 0, 0, TargetType.Enemy, 1,new List<StatusEffect> { rupture }, DamageType.Earth);

        var empower = new StatusEffect("Empowered", StatusEffectType.DamageModifier, 2, 0.2f, null, isDebuff: false);
        var fortify = new Ability(moves[3].name, moves[3].description,AbilityType.Signature, moves[3].cooldown,0, 0, 40, data.SigChargeReq, TargetType.Self, 1,new List<StatusEffect> { empower }, DamageType.None);

        GameCharacter legionary = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq,normal, skill, fortify, passive,data.imageName, data.affiliation, data.lore, data.species);

        return legionary;
}


    private static GameCharacter CreateMizca(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Self, 1);
        // Special-case: PassiveManager adds 15% damage and 10 true damage at end of each round

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            40, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Energy
        );

        var saberQuakeEffects = new List<StatusEffect>
        {
            new StatusEffect("Weakened", StatusEffectType.DamageModifier, 2, -0.2f, null, isDebuff: true, applyChance: 0.3f)
        };

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            20, 0, 0, 0, TargetType.Enemy, 2, saberQuakeEffects, DamageType.Earth
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            55, 0, 0, data.SigChargeReq, TargetType.Enemy, 2, null, DamageType.Earth
        );

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }

    private static GameCharacter CreateNou(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Self, 1);
        // PassiveManager should ensure: Air resistance = 1.0f and dodge chance = 0.4f

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            30, 0, 0, 0, TargetType.Enemy, 2, null, DamageType.Air
        );

        var coveredEffect = new StatusEffect("Covered", StatusEffectType.AccuracyModifier, 1, -0.5f, null,isDebuff: true);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            20, 0, 0, 0, TargetType.Enemy, 1, new List<StatusEffect> { coveredEffect }, DamageType.Air
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            60, 0, 0, data.SigChargeReq, TargetType.Enemy, 3, null, DamageType.Air
        );

        GameCharacter nut = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
        nut.Resistances[DamageType.Air] = 1f;
        return nut;
    }

    private static GameCharacter CreateOlthar(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Ally, 2);
        // PassiveManager: Check all allies at round start, apply +0.5f DamageModifier buff if HP < 100

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            35, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Energy
        );

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            20, 0, 0, 0, TargetType.Enemy, 3, null, DamageType.Air
        );

        var dotEffect = new StatusEffect("Energy Burn", StatusEffectType.DamageOverTime, 2, 15, null, DamageType.Energy,isDebuff: true);
        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            60, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, new List<StatusEffect> { dotEffect }, DamageType.Energy
        );

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }
    private static GameCharacter CreateRover(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passiveEffect = new StatusEffect("Accuracy Boost", StatusEffectType.AccuracyModifier, 999, 0.2f, null);
        var passive = new Ability(
            moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0,
            TargetType.Ally, 3, new List<StatusEffect> { passiveEffect }
        );
        // PassiveManager applies to all allies at round start

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            10, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Lightning
        );

        var debuffEffect = new StatusEffect("Confused", StatusEffectType.AccuracyModifier, 1, -0.15f, null,isDebuff: true);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            15, 0, 0, 0, TargetType.Enemy, 1, new List<StatusEffect> { debuffEffect }, DamageType.Physical
        );

        var resistanceDebuffs = new List<StatusEffect>
        {
            new StatusEffect(name: "Weakened Lightning",type: StatusEffectType.ResistanceModifier,duration: 2,value: -0.3f,source: null,damageType: DamageType.Lightning,isDebuff: true,applyChance: 1f,toDisplay: false),
            new StatusEffect("Weakened Energy", StatusEffectType.ResistanceModifier, 2, -0.3f, null,damageType: DamageType.Energy,isDebuff: true)
        };

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            0, 0, 0, data.SigChargeReq, TargetType.Enemy, 3, resistanceDebuffs
        );

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }

    private static GameCharacter CreateRaish(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(
            moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0,
            TargetType.Self, 1
        );
        // Special-case: PassiveManager listens for ally death and buffs Raish’s damage by 50%

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            20, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Poison
        );

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            30, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Poison
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            0, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, null, DamageType.Poison
        );

        sig.CustomDamageOverride = (user, target) =>
        {
            float raishDmg;
            raishDmg = .5f * (user.MaxHP - user.HP); // Damage = damage taken so far
            return System.Convert.ToInt32(raishDmg);
        };
        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }

    public static GameCharacter CreateRei(CharacterData data)
    {
        // Passive is handled in PassiveManager
        var passive = new Ability("Electric Empress", data.moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Self, 0);

        var normal = new Ability(
            data.moves[1].name, data.moves[1].description, AbilityType.Normal,
            baseCooldown: data.moves[1].cooldown,
            damage: 40, healing: 0, shielding: 0, chargeReq: 0,
            targetType: TargetType.Enemy, maxTargets: 1,
            damageType: DamageType.Lightning
        );

        // Electroshock
        var electroshockEffects = new List<StatusEffect>
        {
            new StatusEffect(name:"Electroshock - Lightning Res Down",type: StatusEffectType.ResistanceModifier, duration: 2, value: -0.2f,source: null, damageType: DamageType.Lightning, isDebuff: true, toDisplay: false),
            new StatusEffect("Electroshock - Water Res Down", StatusEffectType.ResistanceModifier, 2, -0.2f, null,damageType: DamageType.Water, isDebuff: true, toDisplay: false),
            new StatusEffect("Electroshock", StatusEffectType.DamageOverTime, 2, 10f, null, DamageType.Lightning, isDebuff: true)
        };
       
        var skill = new Ability(
            data.moves[2].name, data.moves[2].description, AbilityType.Skill,
            baseCooldown: data.moves[2].cooldown,
            damage: 35, healing: 0, shielding: 0, chargeReq: 0,
            targetType: TargetType.Enemy, maxTargets: 1,
            statusEffectsApplied: electroshockEffects,
            damageType: DamageType.Lightning
        );

        // Signature - 40% chance to stun
        var sigEffects = new List<StatusEffect>
        {
            new StatusEffect("Stunned", StatusEffectType.Stun, 1, 0f, null, DamageType.None, isDebuff: true, applyChance: 0.4f)
        };

        var signature = new Ability(
            data.moves[3].name, data.moves[3].description, AbilityType.Signature,
            baseCooldown: data.moves[3].cooldown,
            damage: 140, healing: 0, shielding: 0, chargeReq: data.SigChargeReq,
            targetType: TargetType.Enemy, maxTargets: 1,
            statusEffectsApplied: sigEffects,
            damageType: DamageType.Lightning
        );

        var rei = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq,
                                    normal, skill, signature, passive, data.imageName, data.affiliation, data.lore, data.species);

        // Flag for override logic in PassiveManager
        //PassiveManager.MarkSpecialOverride("Rei");
        rei.Resistances[DamageType.Lightning] = 1f;

        return rei;
    }

    private static GameCharacter CreateRellin(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(
            moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0,
            TargetType.Self, 1
        );
        // PassiveManager will check incoming lightning damage and convert it to sig charge.

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            30, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Lightning
        );

        var staticDot = new StatusEffect("Static", StatusEffectType.DamageOverTime, 1, 20, null, DamageType.Lightning, isDebuff: true);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            25, 0, 0, 0, TargetType.Enemy, 1, new List<StatusEffect> { staticDot }, DamageType.Lightning
        );

        var shocked = new StatusEffect("Shocked", StatusEffectType.ResistanceModifier, 2, -0.15f, null, DamageType.Lightning, isDebuff: true);
        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            85, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, new List<StatusEffect> { shocked }, DamageType.Lightning
        );

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }

    private static GameCharacter CreateSanguine(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(
            moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0,
            TargetType.Self, 1
        );
        // PassiveManager will handle checking for 2+ DoTs, reducing duration, and granting damage buff.

        var bleedChance = new StatusEffect("Bleed", StatusEffectType.DamageOverTime, 1, 15, null, DamageType.Physical, isDebuff: true, applyChance: 0.1f);
        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            20, 0, 0, 0, TargetType.Enemy, 1, new List<StatusEffect> { bleedChance }, DamageType.Physical
        );

        var bleed = new StatusEffect("Bleed", StatusEffectType.DamageOverTime, 2, 15, null, DamageType.Physical, isDebuff: true);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            25, 0, 0, 0, TargetType.Enemy, 1, new List<StatusEffect> { bleed }, DamageType.Physical
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            60, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, null, DamageType.Physical
        );
        sig.CustomDamageOverride = (user, target) =>
        {
            int dotCount = target.StatusEffects.Count(e => e.Type == StatusEffectType.DamageOverTime);
            return 60 + (20 * dotCount);
        };

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }

    private static GameCharacter CreateSedra(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(
            moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0,
            TargetType.Self, 1
        );
        // Special-case: PassiveManager will check incoming physical damage < 30 and apply 50% ignore chance.

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            40, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Energy
        );

        var burnEffect = new StatusEffect("Solar Wind", StatusEffectType.DamageOverTime, 2, 20, null, DamageType.Energy, isDebuff: true);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            40, 0, 0, 0, TargetType.Enemy, 1, new List<StatusEffect> { burnEffect }, DamageType.Energy
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            50, 0, 0, data.SigChargeReq, TargetType.Enemy, 3, null, DamageType.Energy
        );

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }
    
    private static GameCharacter CreateSkirvex(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(
            moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0,
            TargetType.Self, 1
        );
        // Special-case: On death, trigger AoE poison damage in BattleManager

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            10, 0, 0, 0, TargetType.Enemy, 1,
            new List<StatusEffect> {
                new StatusEffect("Venom", StatusEffectType.DamageOverTime, 2, 20, null, DamageType.Poison, isDebuff: true, applyChance: 0.2f)
            },
            DamageType.Poison
        );

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            25, 0, 0, 0, TargetType.Enemy, 1,
            new List<StatusEffect> {
                new StatusEffect("Decaying", StatusEffectType.DamageModifier, 2, -0.2f, null, DamageType.True, isDebuff: true, applyChance: 0.35f)
            },
            DamageType.Poison
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            0, 0, 0, data.SigChargeReq, TargetType.Enemy, 3,
            new List<StatusEffect> {
                new StatusEffect("Necrosis", StatusEffectType.ResistanceModifier, 2, -0.2f, null, DamageType.Poison, isDebuff: true)
            },
            DamageType.Poison
        );
        // Special-case: Expires HoTs on each target in BattleManager

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }

    private static GameCharacter CreateTempleGuard(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Self, 1);
        

        var normal = new Ability(moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown, 15, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Water);

        var halo = new StatusEffect("Halo", StatusEffectType.ResistanceModifier, 2, 0.2f, null, DamageType.Water, isDebuff: false);
        var halo2 = new StatusEffect("Halo (ice)", StatusEffectType.ResistanceModifier, 2, 0.2f, null, DamageType.Ice, isDebuff: false, toDisplay: false);
        var halo3 = new StatusEffect("Halo (psychic)", StatusEffectType.ResistanceModifier, 2, 0.2f, null, DamageType.Psychic, isDebuff: false, toDisplay: false);
        var clarity = new StatusEffect("Clarity", StatusEffectType.AccuracyModifier, 1, 0.15f, null, isDebuff: false, applyChance: 0.4f);
        var skill = new Ability(moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown, 0, 0, 0, 0, TargetType.Ally, 1, new List<StatusEffect> { halo, halo2, halo3, clarity }, DamageType.None);

        var blind = new StatusEffect("Blinding", StatusEffectType.AccuracyModifier, 1, -0.2f, null, isDebuff: true);
        var sig = new Ability(moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown, 50, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, new List<StatusEffect> { blind }, DamageType.Water);

        GameCharacter guard = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
        return guard;
    }
    private static GameCharacter CreateTRex(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(
            moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0,
            TargetType.Enemy, 3
        );
        // Special-case: PassiveManager checks if target is below 40% HP → +50% bonus damage.

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            40, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Physical
        );

        var hardenedHideEffects = new List<StatusEffect>
        {
            new StatusEffect("Regenerative Hide", StatusEffectType.HealingOverTime, 3, 25, null, isDebuff: false)
        };

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            0, 0, 30, 0, TargetType.Self, 1, hardenedHideEffects
        );


        var poisonEffect = new StatusEffect("Poisoned", StatusEffectType.DamageOverTime, 2, 20, null, DamageType.Poison, isDebuff: true);
        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            50, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, new List<StatusEffect> { poisonEffect }, DamageType.Physical
        );

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }

    private static GameCharacter CreateTrustless(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(
            moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0,
            TargetType.AllyOrSelf, 3
        );
        // PassiveManager will handle applying 20% poison resistance to all allies.

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            18, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Poison
        );

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            15, 0, 0, 0, TargetType.Enemy, 3, null, DamageType.Poison
        );

        var burn = new StatusEffect("Burn", StatusEffectType.DamageOverTime, 1, 20, null, DamageType.Fire, true, 0.2f);
        var poison = new StatusEffect("Poisoned", StatusEffectType.DamageOverTime, 1, 20, null, DamageType.Poison, true, 0.2f);
        var shock = new StatusEffect("Shock", StatusEffectType.DamageOverTime, 1, 20, null, DamageType.Energy, true, 0.2f);
        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            20, 0, 0, data.SigChargeReq, TargetType.Enemy, 1,
            new List<StatusEffect> { burn, poison, shock }, DamageType.Energy
        );

        var Trustless = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
        Trustless.Resistances[DamageType.Poison] = .20f;
        return Trustless;
    }

    private static GameCharacter CreateUlmika(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(
            moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0,
            TargetType.Ally, 2
        );
        // Special-case: PassiveManager removes one debuff from an ally on Yen’s turn — once per round.

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            15, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Air
        );

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            0, 0, 25, 0, TargetType.AllyOrSelf, 2
        );

        var flamingWardEffects = new List<StatusEffect>
        {
            new StatusEffect("Evasion Buff", StatusEffectType.DodgeModifier, 2, 0.20f, null, isDebuff: false, toDisplay: false),
            new StatusEffect("Flame Ward", StatusEffectType.ResistanceModifier, 2, 0.40f, null, isDebuff: false, damageType: DamageType.Fire)
        };

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            0, 0, 0, data.SigChargeReq, TargetType.AllyOrSelf, 1, flamingWardEffects
        );

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }

    private static GameCharacter CreateVasDrel(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(
            moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0,
            TargetType.Self, 1
        );
        // Passive handled in PassiveManager: applies 20% damage resistance to allies vs Riftbeast enemies

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            10, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Water
        );

        var haste = new StatusEffect("Haste", StatusEffectType.DodgeModifier, 1, .15f, null, DamageType.True, isDebuff: false, applyChance: 0.4f);
        var clarity = new StatusEffect("Clarity", StatusEffectType.AccuracyModifier, 1, .15f, null, DamageType.True, isDebuff: false, applyChance: 0.4f);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            0, 0, 0, 0, TargetType.AllyOrSelf, 1, new List<StatusEffect> { haste, clarity }
        );

        var recover = new StatusEffect("Recover", StatusEffectType.HealingOverTime, 2, 20, null, isDebuff: false);
        var reduceSkillCD = new StatusEffect("Runic Acceleration", StatusEffectType.CDModifier, 1, -2, null, toDisplay: false)
        {
            AffectedAbilityType = AbilityType.Skill,
            CooldownChangeAmount = -2
        };

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,0, 0, 0, data.SigChargeReq,TargetType.Ally, 1,
            new List<StatusEffect> { recover, reduceSkillCD },DamageType.Water
        );

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }

    private static GameCharacter CreateVemk(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(
            moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0,
            TargetType.Self, 1
        );
        // Special-case: PassiveManager grants +100% accuracy permanently

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            20, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Poison
        );

        var debuff = new StatusEffect("Dazed", StatusEffectType.AccuracyModifier, 1, -0.25f, null, isDebuff: true);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            15, 0, 0, 0, TargetType.Enemy, 2, new List<StatusEffect> { debuff }
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            25, 0, 0, data.SigChargeReq, TargetType.Enemy, 2, null, DamageType.Psychic
        );
        // Special-case: Signature reduces each target’s sig charge by 30%

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }
    private static GameCharacter CreateVirae(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(
            moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0,
            TargetType.Self, 1
        );
        // Passive: Handled in PassiveManager — converts ice damage into shield

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            15, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Ice
        );

         var iceRes = new StatusEffect("Frozen Aegis", StatusEffectType.ResistanceModifier, 2, 0.2f, null, DamageType.Ice);
    var skill = new Ability(
        moves[2].name,
        moves[2].description,
        AbilityType.Skill,
        moves[2].cooldown,
        0, 0, 25, 0,
        TargetType.Ally,
        1,
        new List<StatusEffect> { iceRes }
    );

    var dmgBuff = new StatusEffect("Glacial Bloom", StatusEffectType.DamageModifier, 2, 0.15f, null,isDebuff: false);
    var extendBuff = new StatusEffect("Extend Buff", StatusEffectType.DurationModifier, 0, 2,null, isDebuff: false);
        extendBuff.DurationTargeting = DurationTargetingMode.SingleBuff;
    var sig = new Ability(
        moves[3].name,moves[3].description,AbilityType.Signature,moves[3].cooldown,0, 0, 0,data.SigChargeReq,TargetType.Ally,1,
        new List<StatusEffect> { dmgBuff, extendBuff }
    );

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }
    private static GameCharacter CreateVyGar(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var rockBuffs = new List<StatusEffect>
        {
            new StatusEffect("Rock Protection", StatusEffectType.ResistanceModifier, 999, 0.1f, null, DamageType.Physical),
            new StatusEffect("Rock Protection (Poison)", StatusEffectType.ResistanceModifier, 999, 0.1f, null, DamageType.Poison,toDisplay: false),
            new StatusEffect("Rock Protection (Earth)", StatusEffectType.ResistanceModifier, 999, 0.1f, null, DamageType.Earth,toDisplay: false),
            new StatusEffect("Rock Protection (Air)", StatusEffectType.ResistanceModifier, 999, 0.1f, null, DamageType.Air,toDisplay: false)
        };
        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0,TargetType.AllyOrSelf, 3, rockBuffs);
        // PassiveManager applies resistances to all allies at start.

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            25, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Physical
        );

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            15, 0, 0, 0, TargetType.Enemy, 3, null, DamageType.Earth
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            0, 0, 120, data.SigChargeReq, TargetType.AllyOrSelf, 1
        );

        var VyGar = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);

        VyGar.Resistances[DamageType.Physical] = .10f;
        VyGar.Resistances[DamageType.Air] = .10f;
        VyGar.Resistances[DamageType.Earth] = .10f;
        VyGar.Resistances[DamageType.Poison] = .10f;
        return VyGar;
    }



    
}
