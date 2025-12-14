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
            EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                .Set("Source", character)
                .Set("Description", "gained 10% elemental damage.")
            );
        }

        if (character.Name == "Bessil")
        {
            foreach (var enemy in character.Enemies)
            {
                enemy.ModifyAccuracy(-.15f);
            }
            EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                .Set("Source", character)
                .Set("Description", "Reduced all enemy accuracy by 15%")
            );
        }
        if (character.Name == "Constellian Trooper")
        {
            int cafAllies = character.Allies.Count(ally => ally.Affil == "Constellian Armed Forces");

            if (cafAllies == 1)
            {
                character.ModifyDamageMultiplier(.10f);
                Debug.Log("Trooper has 1 Constellian ally, gained 10% dmg Bonus");
                EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                    .Set("Source", character)
                    .Set("Description", "1 constellian ally, gained 10% dmg bonus")
                );
            }
            else if (cafAllies >= 2)
            {
                character.ModifyDamageMultiplier(.25f);
                Debug.Log("Trooper has 2 Constellian allies, gained 25% dmg Bonus");
                EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                    .Set("Source", character)
                    .Set("Description", "2 Constellian allies, gained 25% dmg Bonus")
                );
            }
        }
        if (character.Name == "Rover")
            {
                foreach (var ally in character.Allies)
                {
                    ally.ModifyAccuracy(0.20f);
                }
                Debug.Log("R passive applied: +20% accuracy to all allies.");
                EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                    .Set("Source", character)
                    .Set("Description", "+20% accuracy to all allies.")
                );
            }

        if (character.Name == "Trustless Engineer")
        {
            foreach (var ally in character.Allies)
            {
                var resist = new StatusEffect("Poison Safety", StatusEffectType.ResistanceModifier, 99, .2f, character, DamageType.Corrupt, isDebuff: false);
                ally.StatusEffects.Add(resist);
                Debug.Log($"{character.Name} applied Poison Safety to {ally.Name}");
                EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                    .Set("Source", character)
                    .Set("Description", $"applied Poison Safety to {ally.Name}")
                );
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
                EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                    .Set("Source", character)
                    .Set("Description", "Applied shield to IWO and Aetherion Affiliated allies")
                );
            }
        }
        if (character.Name == "Nou")
        {
            character.ModifyDodge(.35f);
            Debug.Log("Nous passive applied: 40% dodge");
            EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                    .Set("Source", character)
                    .Set("Description", "gains 35% dodge")
                );
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
            EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                    .Set("Source", character)
                    .Set("Description", "Rock Protection status effects given to allies")
            );
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
            EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                .Set("Source", character)
                .Set("Description", "Elemental Protection status effects given to allies")
            );
        }
        if (character.Name == "Vemk Parlas")
        {
            character.ModifyAccuracy(1.0f); // +100% accuracy
            Debug.Log("Vemk's passive: Accuracy doubled.");
            EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                .Set("Source", character)
                .Set("Description", "Accuracy doubled")
            );
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
            EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                .Set("Source", character)
                .Set("Description", "15% dmg bonus at start of round and takes 10 true dmg")
            );
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
                        EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                            .Set("Source", character)
                            .Set("Description", $"{ally.Name} gains Cybertron Boost")
                        );
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
                EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                    .Set("Source", character)
                    .Set("Description", "Gained 50 shield due to low HP")
                );
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
                    EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                        .Set("Source", character)
                        .Set("Description", $"removed '{debuff.Name}' from {ally.Name}")
                    );
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
                    DamageType.Elemental
                );

                var fireBuff = new StatusEffect(
                    $"Spartan Instinct (Fire)",
                    StatusEffectType.ResistanceModifier,
                    2,
                    resistanceBonus,
                    character,
                    DamageType.Force
                );

                character.AddStatusEffect(physBuff);
                character.AddStatusEffect(fireBuff);

                //Debug.Log($"Krakoa passive applied: +{resistanceBonus * 100}% Physical & Fire resistance this turn.");
                EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                    .Set("Source", character)
                    .Set("Description", $"+{resistanceBonus * 100}% Elemental & Force resistance this turn.")
                );
            }
        }
        if (character.Name == "Legionary" && !character.HasUsedOneTimePassive)
        {
            float hpThreshold = character.MaxHP * 0.30f;
            if (character.HP <= hpThreshold)
            {
                character.AddShield(50);
                character.MarkOneTimePassive();
                //Debug.Log("Legionary's passive triggered: Gained 50 shield due to low HP.");
                EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                    .Set("Source", character)
                    .Set("Description", "Gained 50 shield due to low HP")
                );
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
                EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                    .Set("Source", character)
                    .Set("Description", "+50% damage and full signature charge")
                );
            }
        }

        if (character.Name == "KAS" && !character.HasUsedOneTimePassive && character.HP < character.MaxHP / 2)
        {
            character.IncreaseCharge(Mathf.RoundToInt(character.SignatureAbility.ChargeRequirement * 0.5f));
            character.MarkOneTimePassive();

            
            EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                .Set("Source", character)
                .Set("Description", "Gained 50% sig charge due to low HP")
            );
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
                EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                    .Set("Source", character)
                    .Set("Description", "DoT durations reduced and damage buff applied")
                );
            }
        }

        

    }



    public static void ApplyOverride(GameCharacter user, GameCharacter target, Ability ability)
    {
        // Rei blocks weak energy attacks
        if (target.Name == "Rei" &&
            ability.DamageType == DamageType.Arcane &&
            ability.Damage <= 30)
        {
            ability.CustomDamageOverride = (u, t) =>
            {
                if (t == target)
                {
                    Debug.Log("Rei's passive blocked the attack.");
                    EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                        .Set("Source", target)
                        .Set("Description", $"Blocked Low damage elemental attack from {user.Name}")
                    );
                    EventManager.Trigger("OnImmunityTriggered", new GameEventData()
                        .Set("Source", target)
                        .Set("Description", $"Blocked Low damage elemental attack from {user.Name}")
                    );  
                    return 0;
                }
                return ability.Damage;
            };
        }

        // Captain Dinso reflects weak energy attacks
        if (target.Name == "Captain Dinso" &&
            ability.DamageType == DamageType.Force &&
            ability.Damage <= 40 &&
            Random.value < 0.5f)
        {
            user.TakeDamage(ability.Damage, ability.DamageType);
            ability.CustomDamageOverride = (u, t) =>
            {
                if (t == target)
                {
                    Debug.Log("Captain Dinso reflected the attack!");
                    EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                        .Set("Source", target)
                        .Set("Description", $"Reflected force attack onto {user.Name}")
                    );
                    EventManager.Trigger("OnImmunityTriggered", new GameEventData()
                        .Set("Source", target)
                        .Set("Description", $"Reflected force attack onto {user.Name}")
                    );  
                    return 0;
                }
                return ability.Damage;
            };
        }

        // Avarice converts Water/Ice damage to healing
        if (target.Name == "Avarice" &&
            ability.DamageType == DamageType.Elemental)
        {
            target.Heal(ability.Damage);
            ability.CustomDamageOverride = (u, t) =>
            {
                if (t == target)
                {
                    Debug.Log("Avarice's passive: Converted Water/Ice damage into healing.");
                    EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                        .Set("Source", target)
                        .Set("Description", "Converted Elemental damage into healing.")
                    );
                     
                    return 0;
                }
                return ability.Damage;
            };
        }

         if (target.Name == "Virae" &&
            (ability.DamageType == DamageType.Elemental))
        {
            target.AddShield(ability.Damage);
            ability.CustomDamageOverride = (u, t) =>
            {
                if (t == target)
                {
                    Debug.Log("Virae's passive: Converted elemental damage into Shielding.");
                    EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                        .Set("Source", target)
                        .Set("Description", "Converted Elemental damage into Shielding.")
                    ); 
                    EventManager.Trigger("OnImmunityTriggered", new GameEventData()
                        .Set("Source", target)
                        .Set("Description", "Converted Elemental damage into Shielding.")
                    );
                    return 0;
                }
                return ability.Damage;
            };
        }
        // Sedra ignores low physical damage (50% chance)
        if (target.Name == "Sedra" &&
            ability.DamageType == DamageType.Force &&
            ability.Damage <= 30 &&
            Random.value < 0.5f)
        {
            ability.CustomDamageOverride = (u, t) =>
            {
                if (t == target)
                {
                    Debug.Log("Sedra's passive: Ignored low physical attack.");
                    EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                        .Set("Source", target)
                        .Set("Description", $"Ignored low force attack from {user.Name}")
                    );
                    EventManager.Trigger("OnImmunityTriggered", new GameEventData()
                        .Set("Source", target)
                        .Set("Description", $"Ignored low force attack from {user.Name}")
                    ); 
                    return 0;
                }
                return ability.Damage;
            };
        }

        // Rellin gains charge from incoming lightning damage
        if (target.Name == "Rellin" &&
            ability.DamageType == DamageType.Arcane &&
            target != user)
        {
            target.IncreaseCharge(ability.Damage);
            Debug.Log($"Rellin gained {ability.Damage} charge from lightning damage");
            EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                .Set("Source", target)
                .Set("Description", $"gained {ability.Damage} charge from arcane damage")
            ); 
            
        }

        
        //Vas drel and his allies take less damage from riftbeasts
        if ((target.Name == "Vas Drel" || target.Allies.Any(ally => ally.Name == "Vas Drel")) &&
            user.Species == "Riftbeast")
        {
            ability.CustomDamageOverride = (u, t) =>
            {
                int reduced = Mathf.RoundToInt(ability.Damage * 0.8f);  // 20% reduction
                Debug.Log($"Vas Drel's passive reduced damage from Riftbeast ({u.Name}) to {reduced} for {t.Name}");
                EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                    .Set("Source", target)
                    .Set("Description", $"reduced damage from Riftbeast ({u.Name}) to {reduced} for {t.Name}")
                );
                return reduced;
            };
        }

    }



    public static void OnCharacterDeath(GameCharacter deadCharacter)
    {
        deadCharacter.deathStatus(true);
        //Debug.Log($"{deadCharacter.Name} marked as dead.");
        
        if (deadCharacter.Name == "Skirvex")
        {
            
            foreach (var enemy in deadCharacter.Enemies)
            {
                enemy.TakeDamage(20, DamageType.Corrupt);
                Debug.Log($"Skirvex's Parastic Birth dealt poison damage to {enemy.Name}");
                EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                    .Set("Source", deadCharacter)
                    .Set("Description", $"dealt poison damage to {enemy.Name}")
                ); 
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
                        EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                            .Set("Source", character)
                            .Set("Description", "+50% damage from ally death.")
                        ); 
                    }
                    break;

                case "Avarice":
                    // Avarice stores the reference for resurrection logic
                    if (character.Allies.Contains(deadCharacter))
                    {
                        ResurrectionTracker.Add(deadCharacter); // Assume ResurrectionTracker is a temp list
                        Debug.Log("Avarice's passive tracked a dead ally for resurrection.");
                        EventManager.Trigger("OnPassiveTriggered", new GameEventData()
                            .Set("Source", character)
                            .Set("Description", "tracked a dead ally for resurrection")
                        ); 
                    }
                    break;
                
            
            }
        }
    }

     public static void ClearResurrectionTracker()
    {
        ResurrectionTracker.Clear();
        //Debug.Log("Resurrection tracker cleared.");
    }


}
