using UnityEngine;
using TMPro;
using System.Collections;
public class UIAnnouncer : MonoBehaviour
{
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private TextMeshProUGUI briefText;
    public static UIAnnouncer Instance;

     private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

       
        // Character creation and team setup will happen here
    }

    void OnEnable()
    {
        EventManager.Subscribe("OnInfoText", HandleInfoText);
        EventManager.Subscribe("OnCharacterDied", ShowBriefDeathInfo);
        EventManager.Subscribe("OnTurnStarted", ShowTurnStartMessage);
        EventManager.Subscribe("OnStatusEffectApplied", ShowStatusEffectText);
        EventManager.Subscribe("OnBurnDownDMG", ShowBurnDownDMG);


    }

    void OnDisable()
    {
        EventManager.Unsubscribe("OnInfoText", HandleInfoText);
        EventManager.Unsubscribe("OnCharacterDied", ShowBriefDeathInfo);
        EventManager.Unsubscribe("OnTurnStarted", ShowTurnStartMessage);
        EventManager.Unsubscribe("OnStatusEffectApplied", ShowStatusEffectText);


    }

    public void DelayedAnnounceAndAdvance(string message, float delay = 3f)
    {
        StartCoroutine(DelayedRoutine(message, delay));
    }

    private IEnumerator DelayedRoutine(string message, float delay)
    {
        yield return new WaitForSeconds(delay);

        GameUI.Announce(message);

        TurnManager.Instance.AdvanceTurn();
    }

    //*******************************************************************************************************************
    // Broadcast UI Methods
    //*******************************************************************************************************************
    private void HandleInfoText(object data)
    {
        string msg = data as string;
        if (!string.IsNullOrEmpty(msg))
        {
            infoText.text = msg;
        }
    }

    private void ShowBriefDeathInfo(object data)
    {
        GameCharacter deadChar = data as GameCharacter;
        if (deadChar != null)
        {
            ShowDeathInfo(deadChar.Name);
        }
    }

    private void ShowTurnStartMessage(object data)
    {
        GameCharacter character = data as GameCharacter;
        if (character == null) return;

        infoText.text = $"{character.Name} is choosing a move.";
    }

    private void ShowStatusEffectText(object data)
    {
        var evt = data as GameEventData;
        if (evt == null) return;

        GameCharacter target = evt.Get<GameCharacter>("Target");
        StatusEffect effect = evt.Get<StatusEffect>("Effect");

        if (target == null || effect == null) return;

        string msg = $"{target.Name} is affected by {effect.Name}.";
        StartCoroutine(ShowBriefMessage(msg));

    }

    private void ShowBurnDownDMG(object data)
    {
        var evt = data as GameEventData;
        if (evt == null) return;

        float percent = evt.Get<float>("Percent");
        bool useMaxHP = evt.Get<bool>("UseMaxHP");

        string basis = useMaxHP ? "Max HP" : "Current HP";

        

        string msg = $"All characters took {percent*100}% of {basis} burndown damage!";
        StartCoroutine(ShowBriefMessage(msg));

    }

    //*******************************************************************************************************************
    //*******************************************************************************************************************
    //*******************************************************************************************************************


    //*******************************************************************************************************************
    // Death Behaviour UI Methods
    //*******************************************************************************************************************
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
    //*******************************************************************************************************************
    //*******************************************************************************************************************
    //*******************************************************************************************************************
    private IEnumerator ShowBriefMessage(string message)
    {
        briefText.text = message;
        StartCoroutine(PopText(briefText));
        yield return new WaitForSeconds(3f);
        briefText.text = "";
    }
}
public static class GameUI
{
    public static void Announce(string message)
    {
        EventManager.Trigger("OnInfoText", message);
    }
}