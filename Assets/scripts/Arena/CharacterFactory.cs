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
                                30, 0, 0, 0, TargetType.Enemy, 1, airburstEffects, DamageType.Elemental);

        
        var skill = new Ability(moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
                                0, 30, 15, 0, TargetType.AllyOrSelf, 1,null,DamageType.Elemental);

        var sig = new Ability(moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
                            100, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, null, DamageType.Elemental);

        var arkhe = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive,data.imageName, data.affiliation, data.lore, data.species);

        // Optional: Give arkhe moderate air resistance
        arkhe.Resistances[DamageType.Elemental] = 0.2f;

        return arkhe;
    }

    private static GameCharacter CreateAvarice(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Self, 1);

        var normal = new Ability(moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
                                30, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Elemental);

        var skill = new Ability(moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
                                0, 75, 0, 0, TargetType.AllyOrSelf, 1,null,DamageType.Elemental);

        var sig = new Ability(moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
                            0, 0, 0, data.SigChargeReq, TargetType.Ally, 1,null,DamageType.Elemental);

        var Avarice = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);


        return Avarice;
    }
    private static GameCharacter CreateBessil(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Self, 1);
        // Passive handled in PassiveManager: reduce all enemy accuracy by 15% at game start

        var normal = new Ability(moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown, 40, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Arcane);

        var skill = new Ability(moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown, 20, 0, 0, 0, TargetType.Enemy, 2, null, DamageType.Arcane);
        skill.CustomDamageOverride = (user, target) =>
        {
            int debuffCount = target.StatusEffects.Count(e => e.IsDebuff && e.ToDisplay);
            return 30 + (10 * debuffCount);
        };

        var stun = new StatusEffect("Stun", StatusEffectType.Stun, 1, 0, null, isDebuff: true, applyChance: 0.5f);
        var sig = new Ability(moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown, 80, 0, 0, data.SigChargeReq, TargetType.Enemy, 2, new List<StatusEffect> { stun }, DamageType.Arcane);

        GameCharacter bessil = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
        return bessil;
    }

    private static GameCharacter CreateBreachSpecialist(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        // Passive: fire resistance to all allies
        var fireRes = new StatusEffect("Insulated Lining", StatusEffectType.ResistanceModifier, 5, 0.15f, null, DamageType.Elemental, isDebuff: false);
        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.AllyOrSelf, 3, new List<StatusEffect> { fireRes });

        // Normal
        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            18, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Elemental
        );

        // Skill
        var accuracyDebuff = new StatusEffect("Blinded", StatusEffectType.AccuracyModifier, 1, -0.10f, null, isDebuff: true);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            10, 0, 0, 0, TargetType.Enemy, 2, new List<StatusEffect> { accuracyDebuff }, DamageType.Elemental
        );

        // Signature
        var burn = new StatusEffect("Burn", StatusEffectType.DamageOverTime, 1, 20, null, DamageType.Elemental, isDebuff: true);
        var afterburn = new StatusEffect("Afterburn", StatusEffectType.ResistanceModifier, 3, -0.20f, null, DamageType.Elemental, isDebuff: true, applyChance: 0.30f);
        var sigEffects = new List<StatusEffect> { burn, afterburn };
        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            10, 0, 0, data.SigChargeReq, TargetType.Enemy, 3, sigEffects, DamageType.Elemental
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
            20, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Force
        );

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            15, 0, 0, 0, TargetType.Enemy, 2, null, DamageType.Force
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            0, 0, 0, data.SigChargeReq, TargetType.Enemy, 3,null,DamageType.Force
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

        var normal = new Ability(moves[1].name, moves[1].description,AbilityType.Normal, moves[1].cooldown,20, 0, 0, 0, TargetType.Enemy, 1,null, DamageType.Force);

        var skill = new Ability(moves[2].name, moves[2].description,AbilityType.Skill, moves[2].cooldown,35, 0, 0, 0, TargetType.Enemy, 1,null, DamageType.Force);

        var sig = new Ability(moves[3].name, moves[3].description,AbilityType.Signature, moves[3].cooldown,22, 0, 0, data.SigChargeReq, TargetType.Enemy, 3,null, DamageType.Force);

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
            25, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Force
        );

        var burnEffect = new StatusEffect("Burn", StatusEffectType.DamageOverTime, 1, 20, null, DamageType.Elemental,isDebuff: true,applyChance: .60f);
        var fireSwingEffects = new List<StatusEffect> { burnEffect };

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            45, 0, 0, 0, TargetType.Enemy, 1, fireSwingEffects, DamageType.Elemental
        );
        // 60% chance to apply Burn — note for BattleManager handling

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            60, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, null, DamageType.Force
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
            50, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Arcane
        );

        var solarFlareEffects = new List<StatusEffect>
        {
            new StatusEffect("Blinded", StatusEffectType.AccuracyModifier, 1, -0.2f, null,isDebuff: true)
        };
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            18, 0, 0, 0, TargetType.Enemy, 3, solarFlareEffects, DamageType.Arcane
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            160, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, null, DamageType.Arcane
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
            20, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Corrupt
        );

        var mockedEffect = new List<StatusEffect>
        {
            new StatusEffect("Mocked", StatusEffectType.DodgeModifier, 1, -0.2f, null,isDebuff: true)
        };

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            0, 0, 0, 0, TargetType.Enemy, 1, mockedEffect,DamageType.Corrupt
        );

        var psychEffects = new List<StatusEffect>
        {
            new StatusEffect("Motivated", StatusEffectType.DamageModifier, 1, 0.3f, null)
        };

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            0, 0, 10, data.SigChargeReq, TargetType.AllyOrSelf, 2, psychEffects,DamageType.Corrupt
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
            40, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Arcane
        );

        // Skill: Dual Wield Precision (2 targets)
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            25, 0, 0, 0, TargetType.Enemy, 2, null, DamageType.Arcane
        );

        // Signature: Sword Skill: Eclipse (100 dmg + 10 shield to self)
        var sigEffects = new List<StatusEffect>
        {
            new StatusEffect("Dazed", StatusEffectType.AccuracyModifier, 1, -0.10f, null,isDebuff: true)
        };

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            100, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, sigEffects, DamageType.Arcane
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
            35, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Elemental
        );

        var burnEffect = new StatusEffect("Chilled", StatusEffectType.DamageOverTime, 2, 20, null, DamageType.Elemental, isDebuff: true);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            30, 0, 0, 0, TargetType.Enemy, 1, new List<StatusEffect> { burnEffect }, DamageType.Elemental
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
            0, 100, 0, data.SigChargeReq, TargetType.Self, 1, healAndRage,DamageType.Corrupt
        );
        // Signature: heals 100, and enrages Krakoa (BattleManager can restrict actions while enraged)

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }

    private static GameCharacter CreateLegionary(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description,AbilityType.Passive, 0, 0, 0, 0, 0,TargetType.Self, 1);
        // Passive will require BattleManager hook: trigger shield once when HP drops below 30%

        var normal = new Ability(moves[1].name, moves[1].description,AbilityType.Normal, moves[1].cooldown,16, 0, 0, 0, TargetType.Enemy, 1,null, DamageType.Force);

        var rupture = new StatusEffect("Rupture", StatusEffectType.DamageOverTime, 1, 20, null, DamageType.Force, isDebuff: true, applyChance: 0.5f);
        var skill = new Ability(moves[2].name, moves[2].description,AbilityType.Skill, moves[2].cooldown,20, 0, 0, 0, TargetType.Enemy, 1,new List<StatusEffect> { rupture }, DamageType.Force);

        var empower = new StatusEffect("Empowered", StatusEffectType.DamageModifier, 2, 0.2f, null, isDebuff: false);
        var fortify = new Ability(moves[3].name, moves[3].description,AbilityType.Signature, moves[3].cooldown,0, 0, 40, data.SigChargeReq, TargetType.Self, 1,new List<StatusEffect> { empower }, DamageType.Elemental);

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
            40, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Corrupt
        );

        var saberQuakeEffects = new List<StatusEffect>
        {
            new StatusEffect("Weakened", StatusEffectType.DamageModifier, 2, -0.2f, null, isDebuff: true, applyChance: 0.3f)
        };

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            20, 0, 0, 0, TargetType.Enemy, 2, saberQuakeEffects, DamageType.Corrupt
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            55, 0, 0, data.SigChargeReq, TargetType.Enemy, 2, null, DamageType.Force
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
            30, 0, 0, 0, TargetType.Enemy, 2, null, DamageType.Elemental
        );

        var coveredEffect = new StatusEffect("Covered", StatusEffectType.AccuracyModifier, 1, -0.5f, null,isDebuff: true);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            20, 0, 0, 0, TargetType.Enemy, 1, new List<StatusEffect> { coveredEffect }, DamageType.Elemental
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            60, 0, 0, data.SigChargeReq, TargetType.Enemy, 3, null, DamageType.Elemental
        );

        GameCharacter nut = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
        
        return nut;
    }

    private static GameCharacter CreateOlthar(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Ally, 2);
        // PassiveManager: Check all allies at round start, apply +0.5f DamageModifier buff if HP < 100

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            35, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Arcane
        );

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            20, 0, 0, 0, TargetType.Enemy, 3, null, DamageType.Force
        );

        var dotEffect = new StatusEffect("Energy Burn", StatusEffectType.DamageOverTime, 2, 15, null, DamageType.Arcane,isDebuff: true);
        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            60, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, new List<StatusEffect> { dotEffect }, DamageType.Arcane
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
            10, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Corrupt
        );

        var debuffEffect = new StatusEffect("Confused", StatusEffectType.AccuracyModifier, 1, -0.15f, null,isDebuff: true);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            15, 0, 0, 0, TargetType.Enemy, 1, new List<StatusEffect> { debuffEffect }, DamageType.Corrupt
        );

        var resistanceDebuffs = new List<StatusEffect>
        {
            new StatusEffect(name: "Hacked Corrupt",type: StatusEffectType.ResistanceModifier,duration: 2,value: -0.3f,source: null,damageType: DamageType.Corrupt,isDebuff: true,applyChance: 1f,toDisplay: false),
            new StatusEffect("Hacked Elemental", StatusEffectType.ResistanceModifier, 2, -0.3f, null,damageType: DamageType.Elemental,isDebuff: true)
        };

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            15, 0, 0, data.SigChargeReq, TargetType.Enemy, 3, resistanceDebuffs,DamageType.Corrupt
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
            20, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Corrupt
        );

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            30, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Corrupt
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            0, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, null, DamageType.Corrupt
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
            damageType: DamageType.Arcane
        );

        // Electroshock
        var electroshockEffects = new List<StatusEffect>
        {
            new StatusEffect(name:"Electroshock - Lightning Res Down",type: StatusEffectType.ResistanceModifier, duration: 2, value: -0.2f,source: null, damageType: DamageType.Arcane, isDebuff: true, toDisplay: false),
           
            new StatusEffect("Electroshock", StatusEffectType.DamageOverTime, 2, 10f, null, DamageType.Arcane, isDebuff: true)
        };
       
        var skill = new Ability(
            data.moves[2].name, data.moves[2].description, AbilityType.Skill,
            baseCooldown: data.moves[2].cooldown,
            damage: 35, healing: 0, shielding: 0, chargeReq: 0,
            targetType: TargetType.Enemy, maxTargets: 1,
            statusEffectsApplied: electroshockEffects,
            damageType: DamageType.Arcane
        );

        // Signature - 40% chance to stun
        var sigEffects = new List<StatusEffect>
        {
            new StatusEffect("Stunned", StatusEffectType.Stun, 1, 0f, null, DamageType.Arcane, isDebuff: true, applyChance: 0.4f)
        };

        var signature = new Ability(
            data.moves[3].name, data.moves[3].description, AbilityType.Signature,
            baseCooldown: data.moves[3].cooldown,
            damage: 140, healing: 0, shielding: 0, chargeReq: data.SigChargeReq,
            targetType: TargetType.Enemy, maxTargets: 1,
            statusEffectsApplied: sigEffects,
            damageType: DamageType.Arcane
        );

        var rei = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq,
                                    normal, skill, signature, passive, data.imageName, data.affiliation, data.lore, data.species);

        
        //rei.Resistances[DamageType.Lightning] = 1f;

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
            30, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Elemental
        );

        var staticDot = new StatusEffect("Static", StatusEffectType.DamageOverTime, 1, 20, null, DamageType.Elemental, isDebuff: true);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            25, 0, 0, 0, TargetType.Enemy, 1, new List<StatusEffect> { staticDot }, DamageType.Elemental
        );

        var shocked = new StatusEffect("Shocked", StatusEffectType.ResistanceModifier, 2, -0.15f, null, DamageType.Elemental, isDebuff: true);
        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            85, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, new List<StatusEffect> { shocked }, DamageType.Elemental
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

        var bleedChance = new StatusEffect("Bleed", StatusEffectType.DamageOverTime, 1, 15, null, DamageType.Force, isDebuff: true, applyChance: 0.1f);
        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            20, 0, 0, 0, TargetType.Enemy, 1, new List<StatusEffect> { bleedChance }, DamageType.Force
        );

        var bleed = new StatusEffect("Bleed", StatusEffectType.DamageOverTime, 2, 15, null, DamageType.Corrupt, isDebuff: true);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            25, 0, 0, 0, TargetType.Enemy, 1, new List<StatusEffect> { bleed }, DamageType.Corrupt
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            60, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, null, DamageType.Corrupt
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
            40, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Force
        );

        var burnEffect = new StatusEffect("Solar Wind", StatusEffectType.DamageOverTime, 2, 20, null, DamageType.Arcane, isDebuff: true);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            40, 0, 0, 0, TargetType.Enemy, 1, new List<StatusEffect> { burnEffect }, DamageType.Arcane
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            50, 0, 0, data.SigChargeReq, TargetType.Enemy, 3, null, DamageType.Arcane
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
                new StatusEffect("Venom", StatusEffectType.DamageOverTime, 2, 20, null, DamageType.Corrupt, isDebuff: true, applyChance: 0.2f)
            },
            DamageType.Corrupt
        );

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            25, 0, 0, 0, TargetType.Enemy, 1,
            new List<StatusEffect> {
                new StatusEffect("Decaying", StatusEffectType.DamageModifier, 2, -0.2f, null, DamageType.Corrupt, isDebuff: true, applyChance: 0.35f)
            },
            DamageType.Corrupt
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            30, 0, 0, data.SigChargeReq, TargetType.Enemy, 3,
            new List<StatusEffect> {
                new StatusEffect("Necrosis", StatusEffectType.ResistanceModifier, 2, -0.2f, null, DamageType.Corrupt, isDebuff: true)
            },
            DamageType.Corrupt
        );
        // Special-case: Expires HoTs on each target in BattleManager

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }

    private static GameCharacter CreateTempleGuard(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0, TargetType.Self, 1);
        

        var normal = new Ability(moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown, 15, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Elemental);

        var halo = new StatusEffect("Halo", StatusEffectType.ResistanceModifier, 2, 0.2f, null, DamageType.Arcane, isDebuff: false);
        var halo2 = new StatusEffect("Halo (ice)", StatusEffectType.ResistanceModifier, 2, 0.2f, null, DamageType.Elemental, isDebuff: false, toDisplay: false);
        var clarity = new StatusEffect("Clarity", StatusEffectType.AccuracyModifier, 1, 0.15f, null, isDebuff: false, applyChance: 0.4f);
        var skill = new Ability(moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown, 0, 0, 0, 0, TargetType.Ally, 1, new List<StatusEffect> { halo, halo2, clarity }, DamageType.Elemental);

        var blind = new StatusEffect("Blinding", StatusEffectType.AccuracyModifier, 1, -0.2f, null, isDebuff: true);
        var sig = new Ability(moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown, 50, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, new List<StatusEffect> { blind }, DamageType.Elemental);

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
            40, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Corrupt
        );

        var hardenedHideEffects = new List<StatusEffect>
        {
            new StatusEffect("Regenerative Hide", StatusEffectType.HealingOverTime, 3, 25, null, isDebuff: false)
        };

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            0, 0, 30, 0, TargetType.Self, 1, hardenedHideEffects,DamageType.Corrupt
        );


        var poisonEffect = new StatusEffect("Poisoned", StatusEffectType.DamageOverTime, 2, 20, null, DamageType.Corrupt, isDebuff: true);
        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            50, 0, 0, data.SigChargeReq, TargetType.Enemy, 1, new List<StatusEffect> { poisonEffect }, DamageType.Corrupt
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
            18, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Corrupt
        );

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            15, 0, 0, 0, TargetType.Enemy, 3, null, DamageType.Corrupt
        );

        var burn = new StatusEffect("Burn", StatusEffectType.DamageOverTime, 1, 20, null, DamageType.Elemental, true, 0.2f);
        var poison = new StatusEffect("Poisoned", StatusEffectType.DamageOverTime, 1, 20, null, DamageType.Corrupt, true, 0.2f);
        var shock = new StatusEffect("Concussed", StatusEffectType.DamageOverTime, 1, 20, null, DamageType.Force, true, 0.2f);
        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            20, 0, 0, data.SigChargeReq, TargetType.Enemy, 1,
            new List<StatusEffect> { burn, poison, shock }, DamageType.Elemental
        );

        var Trustless = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
        Trustless.Resistances[DamageType.Corrupt] = .20f;
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
            15, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Arcane
        );

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            0, 0, 25, 0, TargetType.AllyOrSelf, 2,null,DamageType.Arcane
        );

        var flamingWardEffects = new List<StatusEffect>
        {
            new StatusEffect("Evasion Buff", StatusEffectType.DodgeModifier, 2, 0.20f, null, isDebuff: false, toDisplay: false),
            new StatusEffect("Flame Ward", StatusEffectType.ResistanceModifier, 2, 0.40f, null, isDebuff: false, damageType: DamageType.Corrupt)
        };

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            0, 0, 0, data.SigChargeReq, TargetType.AllyOrSelf, 1, flamingWardEffects,DamageType.Arcane
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
            10, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Arcane
        );

        var haste = new StatusEffect("Haste", StatusEffectType.DodgeModifier, 1, .15f, null, DamageType.Arcane, isDebuff: false, applyChance: 0.4f);
        var clarity = new StatusEffect("Clarity", StatusEffectType.AccuracyModifier, 1, .15f, null, DamageType.Arcane, isDebuff: false, applyChance: 0.4f);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            0, 0, 0, 0, TargetType.AllyOrSelf, 1, new List<StatusEffect> { haste, clarity },DamageType.Arcane
        );

        var recover = new StatusEffect("Recover", StatusEffectType.HealingOverTime, 2, 20, null, isDebuff: false);
        var reduceSkillCD = new StatusEffect("Runic Acceleration", StatusEffectType.CDModifier, 1, -2, null, toDisplay: false)
        {
            AffectedAbilityType = AbilityType.Skill,
            CooldownChangeAmount = -2
        };

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,0, 0, 0, data.SigChargeReq,TargetType.Ally, 1,
            new List<StatusEffect> { recover, reduceSkillCD },DamageType.Arcane
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
            20, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Corrupt
        );

        var debuff = new StatusEffect("Dazed", StatusEffectType.AccuracyModifier, 1, -0.25f, null, isDebuff: true);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            15, 0, 0, 0, TargetType.Enemy, 2, new List<StatusEffect> { debuff }, DamageType.Corrupt
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            25, 0, 0, data.SigChargeReq, TargetType.Enemy, 2, null, DamageType.Corrupt
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
            15, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Elemental
        );

        var iceRes = new StatusEffect("Frozen Aegis", StatusEffectType.ResistanceModifier, 2, 0.2f, null, DamageType.Elemental);
        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            0, 0, 25, 0,TargetType.Ally,1,new List<StatusEffect> { iceRes }
    );

    var dmgBuff = new StatusEffect("Glacial Bloom", StatusEffectType.DamageModifier, 2, 0.15f, null,isDebuff: false);
    var extendBuff = new StatusEffect("Extend Buff", StatusEffectType.DurationModifier, 0, 2,null, isDebuff: false);
        extendBuff.DurationTargeting = DurationTargetingMode.SingleBuff;
    var sig = new Ability(
        moves[3].name,moves[3].description,AbilityType.Signature,moves[3].cooldown,0, 0, 0,data.SigChargeReq,TargetType.Ally,1,
        new List<StatusEffect> { dmgBuff, extendBuff }, DamageType.Elemental
    );

        return new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);
    }
    private static GameCharacter CreateVyGar(CharacterData data)
    {
        List<MoveData> moves = new List<MoveData>(data.moves);

        var rockBuffs = new List<StatusEffect>
        {
            new StatusEffect("Elemental Prot", StatusEffectType.ResistanceModifier, 999, 0.1f, null, DamageType.Elemental,toDisplay: false),
            new StatusEffect("Force Prot", StatusEffectType.ResistanceModifier, 999, 0.1f, null, DamageType.Force,toDisplay: false),
        };
        var passive = new Ability(moves[0].name, moves[0].description, AbilityType.Passive, 0, 0, 0, 0, 0,TargetType.AllyOrSelf, 3, rockBuffs);
        // PassiveManager applies resistances to all allies at start.

        var normal = new Ability(
            moves[1].name, moves[1].description, AbilityType.Normal, moves[1].cooldown,
            25, 0, 0, 0, TargetType.Enemy, 1, null, DamageType.Force
        );

        var skill = new Ability(
            moves[2].name, moves[2].description, AbilityType.Skill, moves[2].cooldown,
            15, 0, 0, 0, TargetType.Enemy, 3, null, DamageType.Force
        );

        var sig = new Ability(
            moves[3].name, moves[3].description, AbilityType.Signature, moves[3].cooldown,
            0, 0, 120, data.SigChargeReq, TargetType.AllyOrSelf, 1,null,DamageType.Force
        );

        var VyGar = new GameCharacter(data.name, data.hp, data.speed, data.SigChargeReq, normal, skill, sig, passive, data.imageName, data.affiliation, data.lore, data.species);

        
        VyGar.Resistances[DamageType.Elemental] = .10f;
        VyGar.Resistances[DamageType.Force] = .10f;
        
        return VyGar;
    }



    
}
