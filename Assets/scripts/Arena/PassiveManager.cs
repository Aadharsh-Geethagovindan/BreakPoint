using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public static class PassiveManager
{

    public static List<GameCharacter> ResurrectionTracker { get; private set; } = new List<GameCharacter>();

    // Called once at game start
    public static void OnGameStart(List<GameCharacter> allCharacters)
    {
        Debug.Log("PassiveManager: OnGameStart triggered.");
        foreach (var character in allCharacters)
        {
            ApplyPersistentPassives(character);
        }
    }

    // Called at the start of each round
    public static void OnRoundStart(List<GameCharacter> allCharacters, int roundNum)
    {
        //Debug.Log("PassiveManager: OnRoundStart triggered.");
        foreach (var character in allCharacters)
        {
            HandleRoundStartPassives(character, roundNum);
        }
    }

    // Called at the start of each character's turn
    public static void OnTurnStart(GameCharacter character)
    {
        //Debug.Log($"PassiveManager: OnTurnStart triggered for {character.Name}");
        HandleTurnStartPassive(character);
    }

    // -- Passive Handlers Below --

    private static void ApplyPersistentPassives(GameCharacter character)
    {
        // For passive effects that apply once (like VyGar's rock resistance buffs)
        if (character.Name == "Arkhe")
        {
            character.ModifyDamageMultiplier(.10f);
            Debug.Log("Arkhe's passive applied: +10% elemental damage.");
            Logger.Instance.PostLog("Arkhe's passive applied: +10% elemental damage.", LogType.Passive);
        }

        if (character.Name == "Bessil")
        {
            foreach (var enemy in character.Enemies)
            {
                enemy.ModifyAccuracy(-.15f);
            }
        }
        if (character.Name == "Constellian Trooper")
        {
            int cafAllies = character.Allies.Count(ally => ally.Affil == "Constellian Armed Forces");

            if (cafAllies == 1)
            {
                character.ModifyDamageMultiplier(.10f);
                Debug.Log("Trooper has 1 Constellian ally, gained 10% dmg Bonus");
                Logger.Instance.PostLog("Trooper has 1 Constellian ally, gained 10% dmg Bonus", LogType.Passive);
            }
            else if (cafAllies >= 2)
            {
                character.ModifyDamageMultiplier(.25f);
                Debug.Log("Trooper has 2 Constellian allies, gained 25% dmg Bonus");
                Logger.Instance.PostLog("Trooper has 2 Constellian allies, gained 25% dmg Bonus", LogType.Passive);
            }
        }
        if (character.Name == "Rover")
            {
                foreach (var ally in character.Allies)
                {
                    ally.ModifyAccuracy(0.20f);
                }
                Debug.Log("R passive applied: +20% accuracy to all allies.");
                Logger.Instance.PostLog("Rover passive applied: +20% accuracy to all allies.", LogType.Passive);
            }

        if (character.Name == "Trustless Engineer")
        {
            foreach (var ally in character.Allies)
            {
                var resist = new StatusEffect("Poison Safety", StatusEffectType.ResistanceModifier, 99, .2f, character, DamageType.Poison, isDebuff: false);
                ally.StatusEffects.Add(resist);
                Debug.Log($"{character.Name} applied Poison Safety to {ally.Name}");
                Logger.Instance.PostLog($"{character.Name} applied Poison Safety to {ally.Name}", LogType.Passive);
            }
        }

        if (character.Name == "Temple Guard")
        {
            foreach (var ally in character.Allies)
            {
                if (ally.Affil == "Council of Ascendance" || ally.Affil == "Aetherion" || ally.Affil == "Intergalactic Wizarding Organization")
                {
                    ally.AddShield(30);
                }
            }
        }
        if (character.Name == "Nou")
        {
            character.ModifyDodge(.35f);
            Debug.Log("Nous passive applied: 40% dodge");
            Logger.Instance.PostLog("Nous passive applied: 40% dodge", LogType.Passive);
        }

        if (character.Name == "VyGar")
        {
            foreach (GameCharacter ally in character.Allies)
            {
                foreach (StatusEffect effect in character.PassiveAbility.StatusEffectsApplied)
                {
                    ally.AddStatusEffect(new StatusEffect(effect)); // clone the effect
                }
            }

            Debug.Log("VyGar's passive applied: Rock Protection status effects given to allies.");
            Logger.Instance.PostLog("VyGar's passive applied: Rock Protection status effects given to allies.", LogType.Passive);
        }

        if (character.Name == "Breach Specialist")
        {
            foreach (GameCharacter ally in character.Allies)
            {
                foreach (StatusEffect effect in character.PassiveAbility.StatusEffectsApplied)
                {
                    ally.AddStatusEffect(new StatusEffect(effect)); // clone the effect
                }
            }

            Debug.Log("Breacher's passive applied:Fire Protection status effects given to allies.");
            Logger.Instance.PostLog("Breacher's passive applied:Fire Protection status effects given to allies.", LogType.Passive);
        }
        if (character.Name == "Vemk Parlas")
        {
            character.ModifyAccuracy(1.0f); // +100% accuracy
            Debug.Log("Vemk's passive: Accuracy doubled.");
            Logger.Instance.PostLog("Vemk's passive: Accuracy doubled.", LogType.Passive);
        }
    }

    private static void HandleRoundStartPassives(GameCharacter character, int roundNum)
    {
        // For passives that trigger each round (like Nou, Mizca, Rover, Olthar)
        if (character.Name == "Mizca")
        {
            if (roundNum <= 1)
            {
                Debug.Log($"Round Number is {roundNum}");
                return;
            }
            if (character.DamageMultiplier <= 1.5)
                {

                    character.ModifyDamageMultiplier(.15f);
                }
            character.TakeDamage(10, DamageType.True);
            Debug.Log("Mizca's passive applied: 15% dmg bonus at start of round and takes 10 true dmg");
            Logger.Instance.PostLog("Mizca's passive applied: 15% dmg bonus at start of round and takes 10 true dmg", LogType.Passive);
            if (character.HP <= 0 && !character.IsDead)
            {
                BattleManager.Instance.HandleDeath(character);
            }
        }

        if (character.Name == "Olthar")
        {
            foreach (var ally in character.Allies)
            {
                if (ally.HP < 200)
                {
                    // Prevent duplicate stacking
                    bool alreadyHasBuff = ally.StatusEffects.Any(e => e.Name == "Cybertron Boost");

                    if (!alreadyHasBuff)
                    {
                        var dmgBuff = new StatusEffect(
                            "Cybertron Boost",
                            StatusEffectType.DamageModifier,
                            1,
                            0.5f,
                            character
                        );

                        ally.AddStatusEffect(dmgBuff);
                        Debug.Log($"Olthar's passive applied: {ally.Name} gains Cybertron Boost.");
                        Logger.Instance.PostLog($"Olthar's passive applied: {ally.Name} gains Cybertron Boost.", LogType.Passive);
                    }
                }
            }
        }

        if (character.Name == "Legionary" && !character.HasUsedOneTimePassive)
        {
            float hpThreshold = character.MaxHP * 0.30f;
            if (character.HP <= hpThreshold)
            {
                character.AddShield(50);
                character.MarkOneTimePassive();
                Debug.Log("Legionary's passive triggered: Gained 50 shield due to low HP.");
                Logger.Instance.PostLog("Legionary's passive triggered: Gained 50 shield due to low HP", LogType.Passive);
            }
        }

        if (character.Name == "Ulmika")
        {
            foreach (var ally in character.Allies)
            {
                var debuff = ally.StatusEffects.FirstOrDefault(e => e.IsDebuff);

                if (debuff != null)
                {
                    ally.RemoveStatusEffect(debuff);
                    Debug.Log($"Ulmika's passive removed '{debuff.Name}' from {ally.Name}");
                    Logger.Instance.PostLog($"Ulmika's passive removed '{debuff.Name}' from {ally.Name}", LogType.Passive);
                    break; // Only remove one effect
                }
            }
        }

        
    }

    private static void HandleTurnStartPassive(GameCharacter character)
    {
        // For turn-specific triggers
        if (character.Name == "Krakoa")
        {
            float healthPercentage = (float)character.HP / character.MaxHP;
            int segmentsLost = Mathf.FloorToInt((1f - healthPercentage) / 0.2f);
            float resistanceBonus = segmentsLost * 0.1f;

            // Remove old versions of this specific passive
            character.StatusEffects.RemoveAll(e =>
                e.Name.StartsWith("Spartan Instinct") && e.Source == character
            );

            if (resistanceBonus > 0)
            {
                var physBuff = new StatusEffect(
                    $"Spartan Instinct (Physical)",
                    StatusEffectType.ResistanceModifier,
                    2,
                    resistanceBonus,
                    character,
                    DamageType.Physical
                );

                var fireBuff = new StatusEffect(
                    $"Spartan Instinct (Fire)",
                    StatusEffectType.ResistanceModifier,
                    2,
                    resistanceBonus,
                    character,
                    DamageType.Fire
                );

                character.AddStatusEffect(physBuff);
                character.AddStatusEffect(fireBuff);

                Debug.Log($"Krakoa passive applied: +{resistanceBonus * 100}% Physical & Fire resistance this turn.");
                Logger.Instance.PostLog($"Krakoa passive applied: +{resistanceBonus * 100}% Physical & Fire resistance this turn.", LogType.Passive);
            }
        }
        if (character.Name == "Legionary" && !character.HasUsedOneTimePassive)
        {
            float hpThreshold = character.MaxHP * 0.30f;
            if (character.HP <= hpThreshold)
            {
                character.AddShield(50);
                character.MarkOneTimePassive();
                Debug.Log("Legionary's passive triggered: Gained 50 shield due to low HP.");
                Logger.Instance.PostLog("Legionary's passive triggered: Gained 50 shield due to low HP.", LogType.Passive);
            }
        }
        if (character.Name == "TRex")
        {
            bool allAlliesDead = character.Allies.All(a => a.IsDead);
            if (allAlliesDead)
            {
                character.ModifyDamageMultiplier(0.5f);
                character.IncreaseCharge(character.SigChargeReq);
                character.MarkOneTimePassive();
                Debug.Log("T-Rex's passive triggered: +50% damage and full signature charge (Territorial Dominance).");
                Logger.Instance.PostLog("T-Rex's passive triggered: +50% damage and full signature charge (Territorial Dominance).", LogType.Passive);
            }
        }

        if (character.Name == "KAS" && !character.HasUsedOneTimePassive && character.HP < character.MaxHP / 2)
        {
            character.IncreaseCharge(Mathf.RoundToInt(character.SignatureAbility.ChargeRequirement * 0.5f));
            character.MarkOneTimePassive();

            
            Logger.Instance.PostLog($"{character.Name}'s Overdrive Matrix activates! +50% Signature Charge", LogType.Passive);
        }

        if (character.Name == "Sanguine")
        {
            int dotCount = character.StatusEffects.Count(eff => eff.Type == StatusEffectType.DamageOverTime);
            if (dotCount >= 2)
            {
                // Reduce duration of all DoTs by 1
                foreach (var eff in character.StatusEffects)
                {
                    if (eff.Type == StatusEffectType.DamageOverTime)
                        eff.TickDuration();
                }

                // Apply 25% damage bonus for 1 turn
                var buff = new StatusEffect("Bloodlust", StatusEffectType.DamageModifier, 2, 0.25f, character, isDebuff: false);
                character.StatusEffects.Add(buff);

                Debug.Log($"{character.Name} triggered Blood Race: DoT durations reduced and damage buff applied.");
                Logger.Instance.PostLog($"{character.Name} triggered Blood Race: DoT durations reduced and damage buff applied.", LogType.Passive);
            }
        }

        

    }



    public static void ApplyOverride(GameCharacter user, GameCharacter target, Ability ability)
    {
        // Rei blocks weak energy attacks
        if (target.Name == "Rei" &&
            ability.DamageType == DamageType.Energy &&
            ability.Damage <= 30)
        {
            ability.CustomDamageOverride = (u, t) =>
            {
                if (t == target)
                {
                    Debug.Log("Rei's passive blocked the attack.");
                    Logger.Instance.PostLog("Rei's passive blocked the attack", LogType.Passive);
                    return 0;
                }
                return ability.Damage;
            };
        }

        // Captain Dinso reflects weak energy attacks
        if (target.Name == "Captain Dinso" &&
            ability.DamageType == DamageType.Energy &&
            ability.Damage <= 40 &&
            Random.value < 0.5f)
        {
            user.TakeDamage(ability.Damage, ability.DamageType);
            ability.CustomDamageOverride = (u, t) =>
            {
                if (t == target)
                {
                    Debug.Log("Captain Dinso reflected the attack!");
                    Logger.Instance.PostLog("Captain Dinso reflected the attack", LogType.Passive);
                    return 0;
                }
                return ability.Damage;
            };
        }

        // Avarice converts Water/Ice damage to healing
        if (target.Name == "Avarice" &&
            (ability.DamageType == DamageType.Water || ability.DamageType == DamageType.Ice))
        {
            target.Heal(ability.Damage);
            ability.CustomDamageOverride = (u, t) =>
            {
                if (t == target)
                {
                    Debug.Log("Avarice's passive: Converted Water/Ice damage into healing.");
                    Logger.Instance.PostLog("Avarice's passive: Converted Water/Ice damage into healing.", LogType.Passive);
                    return 0;
                }
                return ability.Damage;
            };
        }

         if (target.Name == "Virae" &&
            (ability.DamageType == DamageType.Ice))
        {
            target.AddShield(ability.Damage);
            ability.CustomDamageOverride = (u, t) =>
            {
                if (t == target)
                {
                    Debug.Log("Virae's passive: Converted Ice damage into Shielding.");
                    Logger.Instance.PostLog("Virae's passive: Converted Ice damage into Shielding.", LogType.Passive);
                    return 0;
                }
                return ability.Damage;
            };
        }
        // Sedra ignores low physical damage (50% chance)
        if (target.Name == "Sedra" &&
            ability.DamageType == DamageType.Physical &&
            ability.Damage < 30 &&
            Random.value < 0.5f)
        {
            ability.CustomDamageOverride = (u, t) =>
            {
                if (t == target)
                {
                    Debug.Log("Sedra's passive: Ignored low physical attack.");
                    Logger.Instance.PostLog("Sedra's passive: Ignored low physical attack", LogType.Passive);
                    return 0;
                }
                return ability.Damage;
            };
        }

        // Rellin gains charge from incoming lightning damage
        if (target.Name == "Rellin" &&
            ability.DamageType == DamageType.Lightning &&
            target != user)
        {
            target.IncreaseCharge(ability.Damage);
            Debug.Log($"Rellin gained {ability.Damage} charge from lightning damage");
            Logger.Instance.PostLog($"Rellin gained {ability.Damage} charge from lightning damage", LogType.Passive);
        }

        
        //Vas drel and his allies take less damage from riftbeasts
        if ((target.Name == "Vas Drel" || target.Allies.Any(ally => ally.Name == "Vas Drel")) &&
            user.Species == "Riftbeast")
        {
            ability.CustomDamageOverride = (u, t) =>
            {
                int reduced = Mathf.RoundToInt(ability.Damage * 0.8f);  // 20% reduction
                Debug.Log($"Vas Drel's passive reduced damage from Riftbeast ({u.Name}) to {reduced} for {t.Name}");
                Logger.Instance.PostLog($"Vas Drel's passive reduced damage from Riftbeast ({u.Name}) to {reduced} for {t.Name}", LogType.Passive);
                return reduced;
            };
        }

    }



    public static void OnCharacterDeath(GameCharacter deadCharacter)
    {
        deadCharacter.deathStatus(true);
        Debug.Log($"{deadCharacter.Name} marked as dead.");
        Logger.Instance.PostLog($"{deadCharacter.Name} marked as dead.", LogType.Death);

        if (deadCharacter.Name == "Skirvex")
        {
            Debug.Log("Skirvex death case being checked");
            Logger.Instance.PostLog("", LogType.Passive);
            foreach (var enemy in deadCharacter.Enemies)
            {
                enemy.TakeDamage(20, DamageType.Poison);
                Debug.Log($"Skirvex's Parastic Birth dealt poison damage to {enemy.Name}");
                Logger.Instance.PostLog($"Skirvex's Parastic Birth dealt poison damage to {enemy.Name}", LogType.Passive);
            }
        }

        // Combine both teams
            List<GameCharacter> allCharacters = new List<GameCharacter>();
        allCharacters.AddRange(deadCharacter.Allies);
        allCharacters.AddRange(deadCharacter.Enemies);

        foreach (var character in allCharacters)
        {
            
            if (character.IsDead && character != deadCharacter) continue;


            switch (character.Name)
            {
                case "Raish":
                    // Raish gains 50% damage when an ally dies
                    if (character.Allies.Contains(deadCharacter))
                    {
                        character.ModifyDamageMultiplier(0.5f);
                        Debug.Log("Raish's passive triggered: +50% damage from ally death.");
                        Logger.Instance.PostLog("Raish's passive triggered: +50% damage from ally death.", LogType.Passive);
                    }
                    break;

                case "Avarice":
                    // Avarice stores the reference for resurrection logic
                    if (character.Allies.Contains(deadCharacter))
                    {
                        ResurrectionTracker.Add(deadCharacter); // Assume ResurrectionTracker is a temp list
                        Debug.Log("Avarice's passive tracked a dead ally for resurrection.");
                        Logger.Instance.PostLog("Avarice's passive tracked a dead ally for resurrection.", LogType.Passive);
                    }
                    break;
                
                


                // Trex: handled at health threshold — not death
            }
        }
    }

     public static void ClearResurrectionTracker()
    {
        ResurrectionTracker.Clear();
        Debug.Log("Resurrection tracker cleared.");
    }


}
