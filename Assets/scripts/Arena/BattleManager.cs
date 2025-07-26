using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    private List<GameCharacter> allCharacters;
    private ActiveCharPanel charPanel;

    private GameCharacter currentCharacter => TurnManager.Instance.GetCurrentCharacter();

    private List<GameCharacter> charactersInOrder = new List<GameCharacter>();

    [SerializeField] private GameObject characterCardPrefab;
    [SerializeField] private Transform characterHolder;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI briefText;




    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

       charPanel = Object.FindFirstObjectByType<ActiveCharPanel>();
        // Character creation and team setup will happen here
    }



    //Creates GameCharacter Objects for the six selected characters. Also adds them to a dictionary for target lookup
    public void InitializeBattle(CharacterDataArray allCharacterData)
    {
        allCharacters = new List<GameCharacter>();

        List<string> selectedNames = GameData.SelectedCharactersP1 // populate list of all charcaters for the round
            .Concat(GameData.SelectedCharactersP2)
            .Select(c => c.name)
            .ToList();

        foreach (string name in selectedNames)  // create GameCharacterObjects for each char
        {
            GameCharacter character = CharacterFactory.CreateCharacterByName(name, allCharacterData);
            if (character != null)
                allCharacters.Add(character);
            else
                Debug.LogError($"Could not create character: {name}");
        }

        // Now assign allies and enemies for each GameCharacter
        foreach (GameCharacter character in allCharacters)
        {
            foreach (GameCharacter other in allCharacters)
            {
                if (other == character) continue;

                bool sameTeam = GameData.SelectedCharactersP1.Any(c => c.name == character.Name) &&
                                GameData.SelectedCharactersP1.Any(c => c.name == other.Name) ||
                                GameData.SelectedCharactersP2.Any(c => c.name == character.Name) &&
                                GameData.SelectedCharactersP2.Any(c => c.name == other.Name);

                if (sameTeam) character.AddAlly(other);
                else character.AddEnemy(other);
            }
        }

        charactersInOrder = allCharacters.OrderByDescending(c => c.Speed).ToList();
       
    }



    public void ExecuteAbility(GameCharacter user, Ability ability, List<GameCharacter> targets)
    {
        // Check if ability is usable
        if (!ability.IsUsable(user.Charge)) 
        {
            Debug.LogWarning($"{ability.Name} is not usable right now.");
            return;
        }
        // Check Passives
        foreach (GameCharacter target in targets)
        {
            // Passive override logic BEFORE applying the move
            PassiveManager.ApplyOverride(user, target, ability);
        }
        // Apply Ability, clear any overrides
        ability.Apply(user, targets);
        ability.CustomDamageOverride = null;

        // Apply custom logic in abilities
        HandleImmediateAbilityEffects(ability, user, targets); 

        // Create OnAbilityUsed info payload
        var evt = new GameEventData();
        evt.Set("User", user);
        evt.Set("Ability", ability);
        evt.Set("Targets", targets);

        EventManager.Trigger("OnAbilityUsed", evt);

        // Check DEATH
        foreach (var target in targets)
        {
            if (target.HP <= 0)
            {
                HandleDeath(target);
            }
        }

        //  Special Effects (Post-death)
        HandlePostDeathAbilityEffects(ability, user, targets);


        //  Show damage text for first valid target (or loop through all)
        string summary = "";
        foreach (var target in targets)
        {
            if (target.LastDamageTaken > 0)
                summary += $"{target.Name} took {target.LastDamageTaken} damage.\n";
            if (ability.Healing > 0)
                summary += $"{target.Name} healed {ability.Healing} HP.\n";
            if (ability.Shielding > 0)
                summary += $"{target.Name} gained a shield of {ability.Shielding} \n";
            // Optionally include shield or buff info
        }

        GameUI.Announce(summary.Trim());

        // Delay then advance turn
        UIAnnouncer.Instance.DelayedAnnounceAndAdvance($"{TurnManager.Instance.PeekNextCharacter().Name} is choosing a move.");



        
        //Update Status Effect Icons
        foreach (var target in targets)
        {
           CharacterCardUI card = charPanel.FindCardForCharacter(target);
            if (card != null)
            {
                
                card.RefreshStatusEffects(target);
            }
        }
        //TurnManager.Instance.AdvanceTurn();
    }



    public List<GameCharacter> GetTurnOrder()
    {
        return charactersInOrder;
    }


    public void HandleDeath(GameCharacter character)
    {
        if (character.IsDead)
        {
            Debug.LogWarning($"{character.Name} is already marked as dead.");
            return;
        }

        character.deathStatus(true);
        EventManager.Trigger("OnCharacterDied", new GameEventData()
                .Set("Character", character)
            );

        
        // Notify PassiveManager (Ra, Avarice, Trex, etc.)
        PassiveManager.OnCharacterDeath(character);

        // Visually dim the character card
        ActiveCharPanel panel = Object.FindFirstObjectByType<ActiveCharPanel>();
        if (panel != null)
        {
            CharacterCardUI cardUI = panel.FindCardForCharacter(character);
            if (cardUI != null)
            {
                cardUI.SetCardDimmed(true);
                //Debug.Log("Card dimmed");  
            }
        }

        // Check win/loss condition
        if (character.Allies.TrueForAll(a => a.IsDead))
        {
            int winner = GameData.SelectedCharactersP1.Exists(c => c.name == character.Name) ? 2 : 1;

            var evt = new GameEventData();
            evt.Set("Winner", winner);
            EventManager.Trigger("OnGameEnded", evt); // <-- Add this trigger

            PlayerPrefs.SetInt("Winner", winner);
            SceneManager.LoadScene("EndGame");
        }
    }

    public void EndCharacterTurn()
    {
        currentCharacter.HasActedThisTurn = false;
        currentCharacter.ResetRoundFlags(); // includes hasBeenAttacked = false, etc.

        //Debug.Log($"{currentCharacter.Name}'s turn has ended. Advancing...");
        TurnManager.Instance.AdvanceTurn();
    }

    
    

    private void HandleImmediateAbilityEffects(Ability ability, GameCharacter user, List<GameCharacter> targets)
    {
        if (ability.Name == "Necrosis")
        {
            foreach (var target in targets)
            {
                target.StatusEffects.RemoveAll(eff => eff.Type == StatusEffectType.HealingOverTime);
                Debug.Log($"Necrosis removed HoTs from {target.Name}");
            }
        }
        else if (ability.Name == "Constellian Command")
        {
            foreach (var ally in user.Allies)
            {
                int gain = Mathf.RoundToInt(ally.SignatureAbility.ChargeRequirement * 0.5f);
                ally.IncreaseCharge(gain);
                ally.StatusEffects.RemoveAll(e => e.IsDebuff);
            }

            GameUI.Announce($"{user.Name} empowered the team with Constellian Command!");
        }

        if (ability.Name == "Fully Stable Runic Array")
        {
            foreach (var target in targets)
            {
                if (target.SkillAbility.CurrentCooldown > 0)
                {
                    target.SkillAbility.CurrentCooldown = Mathf.Max(0, target.SkillAbility.CurrentCooldown - 2);
                    Debug.Log($"{target.Name}'s Skill cooldown reduced by 2 turns.");
                }
            }
        }

        if (ability.Name == "Engineered Setback")
        {
            foreach (var target in targets)
            {
                int reduction = Mathf.RoundToInt(target.SignatureAbility.ChargeRequirement * 0.3f);
                target.ReduceCharge(reduction);
                Debug.Log($"Vemk reduced {target.Name}'s sig charge by {reduction}");
            }
        }

    }
    

    private void HandlePostDeathAbilityEffects(Ability ability, GameCharacter user, List<GameCharacter> targets)
    {
        if (ability.Name == "Resurrection")
        {
            if (PassiveManager.ResurrectionTracker.Count > 0)
            {
                GameCharacter toRevive = PassiveManager.ResurrectionTracker[0];
                int reviveHP = Mathf.RoundToInt(toRevive.MaxHP * 0.25f);
                toRevive.Heal(reviveHP);
                toRevive.deathStatus(false);
                Debug.Log($"Avarice revived {toRevive.Name} with {reviveHP} HP.");

                EventManager.Trigger("OnCharacterRevived", new GameEventData()
                        .Set("Character", toRevive)
                    );
            }
            else
            {
                Debug.Log("No allies to resurrect with Avarice's ability.");
            }
        }
    }


}
