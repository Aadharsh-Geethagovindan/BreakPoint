using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    private List<GameCharacter> charactersInOrder = new List<GameCharacter>();
    private int currentCharacterIndex = 0;
    private int currentRound = 1;

    private ActiveCharPanel activeCharPanel;
    private GameCharacter currentCharacter;
    [SerializeField] private TextMeshProUGUI roundText;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        SoundManager.Instance.PlayMusic("arena"); // play background song




    }

    private void Start()
    {
        StartCoroutine(InitializeTurnSystem());
    }

    private IEnumerator InitializeTurnSystem()
    {
        ArenaCharacterLoader loader = Object.FindFirstObjectByType<ArenaCharacterLoader>();
        while (loader == null || loader.GetCharacterDataArray() == null || loader.GetCharacterDataArray().characters == null)
        {
            loader = Object.FindFirstObjectByType<ArenaCharacterLoader>();
            yield return null;
        }

        // Initialize characters
        BattleManager.Instance.InitializeBattle(loader.GetCharacterDataArray());
        charactersInOrder = BattleManager.Instance.GetTurnOrder();

        if (charactersInOrder == null || charactersInOrder.Count == 0)
        {
            Debug.LogError("No characters were initialized for the turn order.");
            yield break;
        }

        // Generate UI cards
        loader.CreateCharacterCards(charactersInOrder);

        // Cache UI panel
        activeCharPanel = Object.FindFirstObjectByType<ActiveCharPanel>();
        if (activeCharPanel == null)
        {
            Debug.LogError("ActiveCharPanel not found.");
            yield break;
        }


        //  Apply one-time passives
        PassiveManager.OnGameStart(charactersInOrder);
        StartCoroutine(StartNewRound()); // Begin first round
    }

    private IEnumerator  StartNewRound()
    {
        Debug.Log($"===== ROUND {currentRound} START =====");
        EventManager.Trigger("OnRoundStarted", currentRound);

        ApplyBurndown();

        RecalculateTurnOrder();

        yield return StartCoroutine(AnimationManager.Instance.AnimateCardRepositioning(charactersInOrder));


        foreach (var character in charactersInOrder)
        {
            character.ResetRoundFlags();
            foreach (var ability in new[] { character.NormalAbility, character.SkillAbility, character.SignatureAbility })
            {
                if (ability != null && ability.CurrentCooldown > 0)
                    ability.CurrentCooldown--;
            }

            CharacterCardUI card = activeCharPanel.FindCardForCharacter(character);
            if (card != null)
            {
                //Debug.Log($"Refreshing icons for {character.Name} ");
                card.RefreshStatusEffects(character);
            }


        }
        PassiveManager.ClearResurrectionTracker();
        PassiveManager.OnRoundStart(charactersInOrder, currentRound);
        if (roundText != null)
        {
            roundText.text = $"Round {currentRound}";
        }

        currentCharacterIndex = 0;
        StartTurn();
    }

    private void StartTurn()
    {
        // move to next character, skip dead ones
        while (charactersInOrder[currentCharacterIndex].IsDead)
        {
            currentCharacterIndex = (currentCharacterIndex + 1) % charactersInOrder.Count;
        }
        currentCharacter = charactersInOrder[currentCharacterIndex];
        EventManager.Trigger("OnTurnStarted", currentCharacter);


        //Apply Status effects and update status effect UI
        CharacterCardUI card = activeCharPanel.FindCardForCharacter(currentCharacter);
        if (card != null)
        {
            card.RefreshStatusEffects(currentCharacter);
        }
        StatusEffect.ApplyTurnEffects(currentCharacter);


        foreach (var effect in currentCharacter.StatusEffects.ToList()) // Copy to prevent modification while iterating
        {

            if (effect.Type == StatusEffectType.CDModifier && effect.AffectedAbilityType.HasValue)
            {
                AbilityType type = effect.AffectedAbilityType.Value;
                Ability ability = currentCharacter.GetAbilityOfType(type);
                if (ability != null)
                {
                    if (effect.CooldownChangeAmount > 0)
                        ability.IncreaseCooldown(effect.CooldownChangeAmount);
                    else if (effect.CooldownChangeAmount < 0)
                        ability.ReduceCooldown(-effect.CooldownChangeAmount);
                }
            }
        }

        PassiveManager.OnTurnStart(currentCharacter);

        if (currentCharacter.HP <= 0)
        {
            BattleManager.Instance.HandleDeath(currentCharacter);
            AdvanceTurn(); // Skip to next alive character
            return;
        }


        //activeCharPanel?.DisplayCharacter(currentCharacter);

        if (currentCharacter.HasStatusEffect(StatusEffectType.Stun))
        {
            GameUI.Announce($"{currentCharacter.Name} is stunned and cannot act.");
            UIAnnouncer.Instance.DelayedAnnounceAndAdvance($"{TurnManager.Instance.PeekNextCharacter().Name} is choosing a move.");
            EventManager.Trigger("OnTurnSkipped", currentCharacter);
            return;
        }

    }

    public void AdvanceTurn()
    {
        EventManager.Trigger("OnTurnEnded", currentCharacter);
        TickAndExpireStatusEffects(currentCharacter);
        CharacterCardUI card = activeCharPanel.FindCardForCharacter(currentCharacter);
        if (card != null)
        {
            //Debug.Log($"Refreshing icons for {currentCharacter.Name} ");
            card.RefreshStatusEffects(currentCharacter);
        }
        do
        {
            currentCharacterIndex = (currentCharacterIndex + 1) % charactersInOrder.Count;
        }
        while (charactersInOrder[currentCharacterIndex].IsDead);

        if (currentCharacterIndex == 0)
        {
            currentRound++;
            StartCoroutine(StartNewRound()); // Round complete
        }
        else
        {
            StartTurn();
        }
    }

    public GameCharacter GetCurrentCharacter() => currentCharacter;

    public int GetCurrentRound() => currentRound;

    public void MarkCharacterAsDead(GameCharacter character)
    {
        if (character == currentCharacter)
        {
            AdvanceTurn();
        }
    }

    public void SetCharacterOrder(List<GameCharacter> orderedCharacters)
    {
        charactersInOrder = orderedCharacters;
    }

    private void TickAndExpireStatusEffects(GameCharacter character)
    {
        List<StatusEffect> expired = new List<StatusEffect>();

        foreach (var effect in character.StatusEffects)
        {
            effect.TickDuration();
            if (effect.IsExpired())
            {
                expired.Add(effect);
            }
        }

        foreach (var effect in expired)
        {
            character.RemoveStatusEffect(effect);
            Debug.Log($"{character.Name}'s {effect.Name} expired.");
            EventManager.Trigger("OnStatusEffectExpired", new GameEventData()
                .Set("Target", character)
                .Set("Effect", effect)
            );
        }
    }

    public GameCharacter PeekNextCharacter()
    {
        int nextIndex = (currentCharacterIndex + 1) % charactersInOrder.Count;

        // Find the next non-dead character
        for (int i = 0; i < charactersInOrder.Count; i++)
        {
            GameCharacter nextChar = charactersInOrder[nextIndex];
            if (!nextChar.IsDead)
                return nextChar;

            nextIndex = (nextIndex + 1) % charactersInOrder.Count;
        }

        return null; // All characters dead (shouldn’t happen unless game over)
    }

    private void RecalculateTurnOrder()
    {
        List<GameCharacter> allAlive = charactersInOrder.Where(c => !c.IsDead).ToList();

        var rollResults = new Dictionary<GameCharacter, float>();

        foreach (var character in allAlive)
        {
            int roll = UnityEngine.Random.Range(1, 21); // d20
            float modifier = character.Speed / 4f;
            float total = roll + modifier;
            rollResults[character] = total;

            //Debug.Log($"{character.Name} rolled {roll} + {modifier:F1} = {total:F1}");
        }

        charactersInOrder = rollResults
            .OrderByDescending(pair => pair.Value)
            .Select(pair => pair.Key)
            .ToList();
}


    
    private void ApplyBurndown()
    {
        // Pull tuning 
        int startRound = 12;
        int stepsPerPhase = 2; // rounds each phase lasts
        bool flipToMax = true;
        float[] schedule = { 0.05f, 0.10f, 0.20f, 0.20f, 0.30f, 0.40f };

        if (GameTuning.I != null)
        {
            var d = GameTuning.I.data;
            startRound   = Mathf.Max(0, d.burndownStartRound);
            stepsPerPhase = Mathf.Max(1, d.burndownSteps); // ensure progress
            if (d.burndownPercentSchedule != null && d.burndownPercentSchedule.Length == 6)
                schedule = d.burndownPercentSchedule;
            flipToMax = d.flipToMax;
        }

        int roundsSinceStart = currentRound - startRound;
        if (roundsSinceStart < 0) return;

        int phaseIndex = roundsSinceStart / stepsPerPhase; // integer division
        if (phaseIndex < 0 || phaseIndex > 5)
        {
            phaseIndex = 5;     // stop after all 6 phases
        }
        float damagePercent = Mathf.Clamp01(schedule[phaseIndex]);
        bool useMaxHP = flipToMax && (phaseIndex >= 3);

        foreach (GameCharacter character in charactersInOrder)
        {
            if (character.IsDead) continue;

            int basis = useMaxHP ? character.MaxHP : character.HP;
            int damage = Mathf.CeilToInt(basis * damagePercent);
            if (damage <= 0) continue;

            character.TakeDamage(damage, DamageType.True);

            var evt = new GameEventData();
            evt.Set("Source", null);
            evt.Set("Target", character);
            evt.Set("Damage", damage);
            evt.Set("Type", DamageType.True);
            evt.Set("PhaseIndex", phaseIndex);
            evt.Set("StartRound", startRound);
            evt.Set("Percent", damagePercent);
            evt.Set("UseMaxHP", useMaxHP);
            EventManager.Trigger("OnBurnDownDMG", evt);
        }
    }



}
