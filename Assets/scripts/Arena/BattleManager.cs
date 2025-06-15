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
        /*
        //targeting dictionary lookup
        // Reference to the ActiveCharPanel
        ActiveCharPanel panel = Object.FindFirstObjectByType<ActiveCharPanel>();
        if (panel == null)
        {
            Debug.LogError("ActiveCharPanel not found.");
            return;
        }

        // Create cards and populate lookup
        foreach (GameCharacter character in charactersInOrder)
        {
            GameObject cardObj = GameObject.Instantiate(characterCardPrefab, panel.characterHolder); // <-- you’ll need to expose this
            CharacterCardUI cardUI = cardObj.GetComponent<CharacterCardUI>();
            cardUI.SetCharacter(character);

            panel.RegisterCard(character, cardUI); // <-- you’ll define this method next
        }*/
    }



    public void ExecuteAbility(GameCharacter user, Ability ability, List<GameCharacter> targets)
    {
        //1. Check if ability is usable
        if (!ability.IsUsable(user.Charge)) // ✔ Pass the charge, not the object
        {
            Debug.LogWarning($"{ability.Name} is not usable right now.");
            return;
        }
        //2. Check Passives
        foreach (GameCharacter target in targets)
        {
            // ✅ Passive override logic BEFORE applying the move
            PassiveManager.ApplyOverride(user, target, ability);
        }
        //3. Apply Ability, clear any overrides
        ability.Apply(user, targets);
        ability.CustomDamageOverride = null;

        //4 Apply custom logic in abilities
        HandleImmediateAbilityEffects(ability, user, targets);

        //5. Check DEATH
        foreach (var target in targets)
        {
            if (target.HP <= 0)
            {
                HandleDeath(target);
            }
        }

        // 6. Special Effects (Post-death)
        HandlePostDeathAbilityEffects(ability, user, targets);


        // 🔊 Show damage text for first valid target (or loop through all)
        string summary = "";
        foreach (var target in targets)
        {
            if (target.LastDamageTaken > 0)
                summary += $"{target.Name} took {target.LastDamageTaken} damage.\n";
            else if (ability.Healing > 0)
                summary += $"{target.Name} healed {ability.Healing} HP.\n";
            else if (ability.Shielding > 0)
                summary += $"{target.Name} gained a shield of {ability.Shielding} \n";
            // Optionally include shield or buff info
        }

        SetInfoText(summary.Trim());

        // Delay then advance turn
        StartCoroutine(DelayedInfoAndAdvance());


        

        foreach (var target in targets)
        {
           CharacterCardUI card = charPanel.FindCardForCharacter(target);
            if (card != null)
            {
                Debug.Log("Calling refresh from battle manager");
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
        //Debug.Log($"{character.Name} has died.");
        ShowDeathInfo(character.Name);


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

    public void SetInfoText(string message)
    {
        //if (infoOverrideActive) return; // don't overwrite death message
        if (infoText != null)
            infoText.text = message;
    }

    public void ShowDeathInfo(string name)
    {
        StartCoroutine(BriefDeathInfoRoutine($"{name} has died."));
    }

    private IEnumerator BriefDeathInfoRoutine(string message)
    {
        briefText.text = message;
        StartCoroutine(PopText(briefText));
        yield return new WaitForSeconds(3f);
        briefText.text = "";
    }
    public IEnumerator DelayedInfoAndAdvance()
    {
        yield return new WaitForSeconds(3f);

        GameCharacter next = TurnManager.Instance.PeekNextCharacter();
        if (next != null)
            SetInfoText($"{next.Name} is choosing a move.");

        TurnManager.Instance.AdvanceTurn();
    }

    public IEnumerator PopText(TextMeshProUGUI text, float duration = 0.3f)
    {
        Vector3 originalScale = Vector3.one;
        Vector3 poppedScale = originalScale * 1.4f;

        text.transform.localScale = poppedScale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            text.transform.localScale = Vector3.Lerp(poppedScale, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        text.transform.localScale = originalScale;
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

            SetInfoText($"{user.Name} empowered the team with Constellian Command!");
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
            }
            else
            {
                Debug.Log("No allies to resurrect with Avarice's ability.");
            }
        }
    }


}
