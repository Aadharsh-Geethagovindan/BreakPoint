using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BreakpointChoiceAnimator : MonoBehaviour
{
    [Header("UI References")]
    public GameObject overlay;
    public TextMeshProUGUI resultText;

    [Header("Animation Settings")]
    public float flashDuration = 2.5f;  // total time flashing between effects
    public float flashInterval = 0.2f;  // speed of text change
    public float holdTime = 1.5f;       // time to show the final choice

    private int currentTeamId;

    // These will be randomized through when selected
    private readonly string[] buffEffects = new[]
    {
        "Essence Surge (+25% Damage)",
        "Barrier Pulse (+30% Max HP Shield)",
        "Critical Flow (+30% Crit Chance)",
        "Sig Overcharge (+40 Sig Charge)"
    };

    private readonly string[] debuffEffects = new[]
    {
        "Essence Drain (-25% Enemy Damage)",
        "Withering Weight (-20% Enemy Speed)",
        "Sig Disrupt (-30 Sig Charge to Enemies)",
        "Weakened Resolve (-20% Enemy Accuracy)"
    };

    public void SetTeamId(int id) => currentTeamId = id;

    public void MakeChoice(string choice)
    {
        // Instead of hiding immediately, start animation coroutine
        StartCoroutine(AnimateChoiceSequence(choice));
    }

    private IEnumerator AnimateChoiceSequence(string choice)
    {
        overlay.SetActive(true);
        resultText.gameObject.SetActive(true);
        resultText.text = "";

        string[] pool = choice == "Buff" ? buffEffects : debuffEffects;
        string finalChoice = pool[Random.Range(0, pool.Length)];

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            resultText.text = pool[Random.Range(0, pool.Length)];
            elapsed += flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }

        // Show the final one and hold
        resultText.text = finalChoice;
        yield return new WaitForSeconds(holdTime);

        // Fade out overlay and text (optional smooth exit)
        overlay.SetActive(false);

        // Fire event like before
        EventManager.Trigger("OnBreakpointChoiceSelected",
            new GameEventData()
                .Set("TeamId", currentTeamId)
                .Set("Choice", choice)
                .Set("Result", finalChoice));

        resultText.text = "";
        resultText.gameObject.SetActive(false);
    }
}
