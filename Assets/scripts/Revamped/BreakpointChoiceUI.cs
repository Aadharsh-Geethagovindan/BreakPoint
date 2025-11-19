using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BreakpointChoiceUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject overlay;
    [SerializeField] private Button buffButton;
    [SerializeField] private Button debuffButton;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("Timing")]
    [SerializeField] private float flashDuration = 2.5f;
    [SerializeField] private float flashInterval = 0.20f;
    [SerializeField] private float holdTime      = 1.5f;

    private int currentTeamId;

    // Pools (text is what EffectManager parses)
    private static readonly string[] BuffPool = {
        "Essence Surge (+25% Damage)",
        "Barrier Pulse (+30% Max HP Shield)",
        "Critical Flow (+30% Crit Chance)",
        "Sig Overcharge (+40 Sig Charge)"
    };
    private static readonly string[] DebuffPool = {
        "Essence Drain (-25% Enemy Damage)",
        "Withering Weight (-20% Enemy Speed)",
        "Sig Disrupt (-30 Sig Charge to Enemies)",
        "Weakened Resolve (-20% Enemy Accuracy)"
    };

    void Awake()
    {
        if (overlay) overlay.SetActive(false);
        if (resultText) resultText.gameObject.SetActive(false);

        EventManager.Subscribe("OnBreakpointTriggered", HandleTriggered);
    }

    void OnDestroy()
    {
        EventManager.Unsubscribe("OnBreakpointTriggered", HandleTriggered);
    }

    void Start()
    {
        if (buffButton)   buffButton.onClick.AddListener(() => OnChoiceClicked("Buff"));
        if (debuffButton) debuffButton.onClick.AddListener(() => OnChoiceClicked("Debuff"));
    }

    private void HandleTriggered(object payload)
    {
        if (payload is not GameEventData evt) return;
        currentTeamId = evt.Get<int>("TeamId");
        if (overlay) overlay.SetActive(true);
        if (resultText) { resultText.text = ""; resultText.gameObject.SetActive(true); }
        SetButtonsInteractable(true);
    }

    private void OnChoiceClicked(string choice)
    {
        // prevent double-click during animation
        SetButtonsInteractable(false);
        StartCoroutine(AnimateChoiceSequence(choice));
    }

    private IEnumerator AnimateChoiceSequence(string choice)
    {
        string[] pool = (choice == "Buff") ? BuffPool : DebuffPool;
        string finalChoice = pool[Random.Range(0, pool.Length)];

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            if (resultText) resultText.text = pool[Random.Range(0, pool.Length)];
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        if (resultText) resultText.text = finalChoice;
        yield return new WaitForSeconds(holdTime);

        // Close UI
        if (overlay) overlay.SetActive(false);
        if (resultText) { resultText.text = ""; resultText.gameObject.SetActive(false); }

        // Notify effects manager with team id + final choice
        EventManager.Trigger("OnBreakpointChoiceSelected",
            new GameEventData()
                .Set("TeamId", currentTeamId)
                .Set("Choice", choice)
                .Set("Result", finalChoice));
    }

    private void SetButtonsInteractable(bool v)
    {
        if (buffButton) buffButton.interactable = v;
        if (debuffButton) debuffButton.interactable = v;
    }
}