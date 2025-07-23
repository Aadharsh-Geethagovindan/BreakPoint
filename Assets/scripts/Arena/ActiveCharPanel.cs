using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class ActiveCharPanel : MonoBehaviour
{
    [Header("UI Components")]
    public Image characterImage;
    public Slider hpBar;
    public TextMeshProUGUI hpText;
    public Slider sigChargeBar;
    public TextMeshProUGUI sigChargeText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI damageMultiplierText;
    public Button move1Button;
    public Button move2Button;
    public Button move3Button;
    public TextMeshProUGUI passiveDisplayText;
    public TextMeshProUGUI normalDisplayText;
    public TextMeshProUGUI skillDisplayText;
    public TextMeshProUGUI sigDisplayText;
    public TextMeshProUGUI statusEffectsText;
    public Button confirmButton;
    public Button reselectButton;

    private GameCharacter currentGameCharacter;
    private List<GameCharacter> selectedTargets = new List<GameCharacter>();
    private List<GameCharacter> validTargets = new List<GameCharacter>();
    public Transform characterHolder; // Drag the holder from scene
    private Dictionary<GameCharacter, CharacterCardUI> cardLookup = new Dictionary<GameCharacter, CharacterCardUI>();



    //private CharacterData currentCharacter;
    private Ability selectedAbility;

    private void Start()
    {
        //Debug.Log("Panel height: " + GetComponent<RectTransform>().rect.height);
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmButtonClicked);

        reselectButton.onClick.RemoveAllListeners();
        reselectButton.onClick.AddListener(OnReselectClicked);

    }

    public void DisplayCharacter(GameCharacter character)
    {
        //Debug.Log($"Displaying character: {character.Name}");

        // Store live reference
        currentGameCharacter = character;

        // Fetch static character data for image & move descriptions
        ArenaCharacterLoader loader = Object.FindFirstObjectByType<ArenaCharacterLoader>();
        if (loader == null)
        {
            Debug.LogError("ArenaCharacterLoader not found.");
            return;
        }

        CharacterDataArray dataArray = loader.GetCharacterDataArray();
        CharacterData matchingData = dataArray.characters.FirstOrDefault(c => c.name == character.Name);

        if (matchingData == null)
        {
            Debug.LogError($"No CharacterData found for {character.Name}");
            return;
        }

        // === IMAGE ===
        if (characterImage != null)
        {
            characterImage.sprite = Resources.Load<Sprite>($"Images/{matchingData.imageName}");
            //Debug.Log($"Character Image Loaded: {matchingData.imageName}");
        }

        // === HP ===
        if (hpBar != null && hpText != null)
        {
            hpBar.maxValue = character.MaxHP;
            hpBar.value = character.HP;
            hpText.text = $"{character.HP}/{character.MaxHP}";
        }

        // === SIG CHARGE ===
        if (sigChargeBar != null && sigChargeText != null)
        {
            sigChargeBar.maxValue = character.SigChargeReq;
            sigChargeBar.value = character.Charge;
            sigChargeText.text = $"{character.Charge}/{character.SigChargeReq}";
        }
    

        // === MOVE DESCRIPTIONS (SPLIT) ===
        if (matchingData.moves != null && matchingData.moves.Length == 4)
        {
            if (passiveDisplayText != null)
                passiveDisplayText.text = $"{matchingData.moves[0].name}: {matchingData.moves[0].description}";

            if (normalDisplayText != null)
                normalDisplayText.text = $"{matchingData.moves[1].name}: {matchingData.moves[1].description} (Cooldown: {matchingData.moves[1].cooldown})";

            if (skillDisplayText != null)
                skillDisplayText.text = $"{matchingData.moves[2].name}: {matchingData.moves[2].description} (Cooldown: {matchingData.moves[2].cooldown})";

            if (sigDisplayText != null)
                sigDisplayText.text = $"{matchingData.moves[3].name}: {matchingData.moves[3].description} (Cooldown: {matchingData.moves[3].cooldown})";
        }

        // === ACCURACY / DMG MULT ===
        if (accuracyText != null && damageMultiplierText != null)
        {
            accuracyText.text = $"Accuracy: \n{Mathf.RoundToInt(character.GetModifiedAccuracy() * 100)}%";
            damageMultiplierText.text = $"Dmg Multiplier: \n {character.GetModifiedDamageMultiplier()}x";
        }

        // === STATUS EFFECTS ==
        if (statusEffectsText != null)
        {
            string statusText = "";

            foreach (StatusEffect effect in character.StatusEffects)
            {
                if (effect.ToDisplay)
                {
                    string color = effect.IsDebuff ? "#ff4d4d" : "#4dff4d"; // Red for debuffs, green for buffs
                    statusText += $"<color={color}>{effect.Name} ({effect.Duration})</color>\n";
                    //Debug.Log(effect.ToDisplay);
                }
            }

            statusEffectsText.text = statusText.TrimEnd(); // Remove last newline if needed
        }

        // === BUTTONS ===
        SetupMoveButton(move1Button, 0); // Normal
        SetupMoveButton(move2Button, 1); // Skill
        SetupMoveButton(move3Button, 2); // Signature

        //Set Button Text
        // Example for Normal ability button
        int normalCD = character.NormalAbility.CurrentCooldown;
        move1Button.GetComponentInChildren<TextMeshProUGUI>().text = (normalCD == 0) ? "USE" : normalCD.ToString();
        
        // === SKILL BUTTON ===
        int skillCD = character.SkillAbility.CurrentCooldown;
        TextMeshProUGUI skillText = move2Button.GetComponentInChildren<TextMeshProUGUI>();
        Image skillImage = move2Button.GetComponent<Image>();

        if (skillCD == 0)
        {
            skillText.text = "USE";
            skillText.color = Color.white;
            skillImage.color = new Color(1f, 1f, 1f, 1f); // Fully visible
        }
        else
        {
            skillText.text = skillCD.ToString();
            skillText.color = Color.red;
            skillImage.color = new Color(0.6f, 0.4f, 0.6f, 0.6f); // Slightly dimmed purple tone
        }

        // === SIG BUTTON ===
        int sigCD = character.SignatureAbility.CurrentCooldown;
        TextMeshProUGUI sigText = move3Button.GetComponentInChildren<TextMeshProUGUI>();
        Image sigImage = move3Button.GetComponent<Image>();

        bool sigReady = sigCD == 0 && character.Charge >= character.SignatureAbility.ChargeRequirement;

        if (sigReady)
        {
            sigText.text = "USE";
            sigText.color = Color.white;
            sigImage.color = new Color(1f, 1f, 1f, 1f); // Fully visible
        }
        else
        {
            sigText.text = $"{character.Charge}/{character.SignatureAbility.ChargeRequirement}";
            sigText.color = Color.red;
            sigImage.color = new Color(0.6f, 0.4f, 0.6f, 0.6f); // Slightly dimmed purple tone
        }
        
        /*
        int skillCD = character.SkillAbility.CurrentCooldown;
        move2Button.GetComponentInChildren<TextMeshProUGUI>().text = (skillCD == 0) ? "USE" : skillCD.ToString();

        int sigCD = character.SignatureAbility.CurrentCooldown;
        move3Button.GetComponentInChildren<TextMeshProUGUI>().text = (sigCD == 0 && character.Charge >= character.SignatureAbility.ChargeRequirement)
                                                                    ? "USE"
                                                                    : $"{character.Charge}/{character.SignatureAbility.ChargeRequirement}";
            */


    }

    private void SetupMoveButton(Button button, int index)
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => UseMove(index));
        }
    }


    private void UseMove(int index)
    {
         SoundManager.Instance.PlaySFX("click");
         ClearTargetingState();
         
         switch (index)
        {
            case 0:
                selectedAbility = currentGameCharacter.NormalAbility;
                break;
            case 1:
                selectedAbility = currentGameCharacter.SkillAbility;
                break;
            case 2:
                selectedAbility = currentGameCharacter.SignatureAbility;
                break;
            default:
                Debug.LogWarning("Invalid move index.");
                return;
        }
        GameUI.Announce($"{currentGameCharacter.Name} selected {selectedAbility.Name}. Max Targets: {selectedAbility.MaxTargets}");


        //Debug.Log($"Selected move: {selectedAbility.Name} ({selectedAbility.AbilityType})");
        // 🔥 Start target selection
        BeginTargetSelection();


    }

    private void BeginTargetSelection()
    {
        selectedTargets.Clear();
        validTargets.Clear();

        if (selectedAbility == null || currentGameCharacter == null)
        {
            Debug.LogError("Cannot begin target selection. Ability or character is null.");
            return;
        }

        // Determine who is valid based on targeting type
        switch (selectedAbility.TargetType)
        {
            case TargetType.Enemy:
                validTargets = currentGameCharacter.Enemies
                    .Where(c => !c.IsDead)
                    .ToList();
                break;

            case TargetType.Ally:
                if (currentGameCharacter.Name == "Avarice" && selectedAbility.Name == "Resurrection")
                {
                    // Only show allies in resurrection tracker
                    validTargets = PassiveManager.ResurrectionTracker
                        .Where(c => currentGameCharacter.Allies.Contains(c))
                        .ToList();
                }
                else
                {
                    validTargets = currentGameCharacter.Allies
                        .Where(c => !c.IsDead)
                        .ToList();
                }
                break;
            case TargetType.AllyOrSelf:
            validTargets = currentGameCharacter.Allies
                .Where(c => !c.IsDead)
                .ToList();

            if (!currentGameCharacter.IsDead && !validTargets.Contains(currentGameCharacter))
            {
                validTargets.Add(currentGameCharacter);
            }
            break;
            case TargetType.Self:
                // Only add self if not dead
                validTargets = currentGameCharacter.IsDead ? new List<GameCharacter>() : new List<GameCharacter> { currentGameCharacter };
                break;

            case TargetType.All:
                validTargets = new List<GameCharacter>();
                validTargets.AddRange(currentGameCharacter.Allies.Where(c => !c.IsDead));
                validTargets.AddRange(currentGameCharacter.Enemies.Where(c => !c.IsDead));
                if (!currentGameCharacter.IsDead)
                    validTargets.Add(currentGameCharacter);
                break;
        }


        // Highlight UI of valid targets
        foreach (GameCharacter target in validTargets)
        {
            CharacterCardUI cardUI = FindCardForCharacter(target);
            if (cardUI != null)
            {
                cardUI.SetTargetHighlight(true);  // e.g., glow or outline effect
                cardUI.SetClickable(true, OnTargetClicked);
            }
        }
    }

    public CharacterCardUI FindCardForCharacter(GameCharacter character)
    {
        if (cardLookup.TryGetValue(character, out var cardUI))
            return cardUI;

        Debug.LogWarning($"Card for {character.Name} not found.");
        return null;
    }

    public void RegisterCard(GameCharacter character, CharacterCardUI cardUI)
    {
        if (!cardLookup.ContainsKey(character))
        {
            cardLookup.Add(character, cardUI);
        }
    }

    private void OnTargetClicked(GameCharacter target)
    {
        SoundManager.Instance.PlaySFX("click");
        if (!validTargets.Contains(target) || selectedTargets.Contains(target))
            return;

        selectedTargets.Add(target);
        //Debug.Log($"Target selected: {target.Name}");

        // If max targets reached, auto-confirm or wait for manual confirmation
        if (selectedTargets.Count >= selectedAbility.MaxTargets)
        {
            //Debug.Log("Max targets selected.");
            // Optional: Auto-enable confirm button here
            foreach (GameCharacter selectedTarget in validTargets)
            {
                // Disable clicking on all remaining targets
                CharacterCardUI cardUI = FindCardForCharacter(selectedTarget);
                if (cardUI != null)
                {
                    //Debug.Log("Disabling non clicked characters");
                    cardUI.SetClickable(false, null);
                }

                if (!selectedTargets.Contains(selectedTarget))
                {
                    //Debug.Log("Setting non clicked characters to dimmed");
                    cardUI?.SetOutlineDimmed(true); // visually dim only the non selected characters
                }
            }

        }
    }

    public void OnConfirmButtonClicked()
    {
        SoundManager.Instance.PlaySFX("click");
        if (selectedAbility == null || selectedTargets.Count == 0)
        {
            Debug.LogWarning("No ability or targets selected.");
            return;
        }

        BattleManager.Instance.ExecuteAbility(currentGameCharacter, selectedAbility, selectedTargets);
        ClearTargetingState();
    }
    private void OnReselectClicked()
    {
        SoundManager.Instance.PlaySFX("click");

         // ✅ Preserve ability and character before clearing
        var preservedAbility = selectedAbility;
        var preservedCharacter = currentGameCharacter;
        ClearTargetingState();

        // ✅ Restore them before re-targeting
        selectedAbility = preservedAbility;
        currentGameCharacter = preservedCharacter;
        BeginTargetSelection(); // Restart targeting
        Debug.Log("Targeting reset.");
    }


    private void ClearTargetingState()
    {
        foreach (GameCharacter target in validTargets)
        {
            CharacterCardUI cardUI = FindCardForCharacter(target);
            if (cardUI != null)
            {
                cardUI.SetTargetHighlight(false);
                cardUI.SetClickable(false, null);
                cardUI.SetOutlineDimmed(false);
            }
        }

        selectedTargets.Clear();
        validTargets.Clear();
        selectedAbility = null;
        
    }


    public void ClearPanel()
    {
        currentGameCharacter = null;
        characterImage.sprite = null;
        hpText.text = "";
        sigChargeText.text = "";
        accuracyText.text = "";
        damageMultiplierText.text = "";
    }
}

