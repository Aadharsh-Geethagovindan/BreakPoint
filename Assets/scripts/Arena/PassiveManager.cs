using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
    public static void OnRoundStart(List<GameCharacter> allCharacters)
    {
        //Debug.Log("PassiveManager: OnRoundStart triggered.");
        foreach (var character in allCharacters)
        {
            HandleRoundStartPassives(character);
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
        }

        if (character.Name == "Rover")
        {
            foreach (var ally in character.Allies)
            {
                ally.ModifyAccuracy(0.20f);
            }
            Debug.Log("R passive applied: +20% accuracy to all allies.");
        }

        if (character.Name == "Trustless Engineer")
        {
            foreach (var ally in character.Allies)
            {
                var resist = new StatusEffect("Poison Safety", StatusEffectType.ResistanceModifier, 99, .2f, character, DamageType.Poison, isDebuff: false);
                ally.StatusEffects.Add(resist);
                Debug.Log($"{character.Name} applied Poison Safety to {ally.Name}");
            }
        }


        if (character.Name == "Nou")
        {
            character.ModifyDodge(.35f);
            Debug.Log("Nous passive applied: 40% dodge");
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
        }

        if (character.Name == "Vemk Parlas")
        {
            character.ModifyAccuracy(1.0f); // +100% accuracy
            Debug.Log("Vemk's passive: Accuracy doubled.");
        }
    }

    private static void HandleRoundStartPassives(GameCharacter character)
    {
        // For passives that trigger each round (like Nou, Mizca, Rover, Olthar)
        if (character.Name == "Mizca")
        {
            if (character.DamageMultiplier <= 1.5)
            {
                character.ModifyDamageMultiplier(.15f);
            }
            character.TakeDamage(10, DamageType.True);
            Debug.Log("Mizca's passive applied: 15% dmg bonus at start of round and takes 10 true dmg");
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
                    }
                }
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
            }
        }

        if (character.Name == "KAS" && !character.HasUsedOneTimePassive && character.HP < character.MaxHP / 2)
        {
            character.IncreaseCharge(Mathf.RoundToInt(character.SignatureAbility.ChargeRequirement * 0.5f));
            character.MarkOneTimePassive();

            BattleManager.Instance.SetInfoText($"{character.Name}'s Overdrive Matrix activates! +50% Signature Charge");
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
        }

        // IMPORTANT - UPDATE THIS LINE AFTER ADDING SPECIES TO JSON
        //Vas drel and his allies take less damage from riftbeasts
        if ((target.Name == "Vas Drel" || target.Allies.Any(ally => ally.Name == "Vas Drel")) &&
            user.Name == "Riftbeast")
        {
            ability.CustomDamageOverride = (u, t) =>
            {
                int reduced = Mathf.RoundToInt(ability.Damage * 0.8f);  // 20% reduction
                Debug.Log($"Vas Drel's passive reduced damage from Riftbeast ({u.Name}) to {reduced} for {t.Name}");
                return reduced;
            };
        }

    }



    public static void OnCharacterDeath(GameCharacter deadCharacter)
    {
        deadCharacter.deathStatus(true);
        Debug.Log($"{deadCharacter.Name} marked as dead.");

        // Combine both teams
        List<GameCharacter> allCharacters = new List<GameCharacter>();
        allCharacters.AddRange(deadCharacter.Allies);
        allCharacters.AddRange(deadCharacter.Enemies);

        foreach (var character in allCharacters)
        {
            if (character.IsDead) continue;

            switch (character.Name)
            {
                case "Raish":
                    // Raish gains 50% damage when an ally dies
                    if (character.Allies.Contains(deadCharacter))
                    {
                        character.ModifyDamageMultiplier(0.5f);
                        Debug.Log("Raish's passive triggered: +50% damage from ally death.");
                    }
                    break;

                case "Avarice":
                    // Avarice stores the reference for resurrection logic
                    if (character.Allies.Contains(deadCharacter))
                    {
                        ResurrectionTracker.Add(deadCharacter); // Assume ResurrectionTracker is a temp list
                        Debug.Log("Avarice's passive tracked a dead ally for resurrection.");
                    }
                    break;
                case "Skirvex":
                
                    foreach (var enemy in character.Enemies)
                    {
                        enemy.TakeDamage(20, DamageType.Poison);
                        Debug.Log($"Skirvex's Parastic Birth dealt 20 poison damage to {enemy.Name}");
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
