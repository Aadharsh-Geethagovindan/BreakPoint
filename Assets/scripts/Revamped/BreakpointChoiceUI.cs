using UnityEngine;
using UnityEngine.UI;

public class BreakpointChoiceUI : MonoBehaviour
{
    [SerializeField] private GameObject overlay;
    [SerializeField] private Button buffButton;
    [SerializeField] private Button debuffButton;

    private int currentTeamId;

    private void Awake()
    {
        overlay.SetActive(false);
        EventManager.Subscribe("OnBreakpointTriggered", HandleBreakpointTriggered);
    }

    private void OnDestroy()
    {
        EventManager.Unsubscribe("OnBreakpointTriggered", HandleBreakpointTriggered);
    }

    private void HandleBreakpointTriggered(object data)
    {
        Debug.Log($"Breakpoint triggered,overlay being set");
        if (data is not GameEventData evt) return;
        currentTeamId = evt.Get<int>("TeamId");
        overlay.SetActive(true);
    }

    private void Start()
    {
        buffButton.onClick.AddListener(() => MakeChoice("Buff"));
        debuffButton.onClick.AddListener(() => MakeChoice("Debuff"));
    }

    private void MakeChoice(string choice)
    {
        overlay.SetActive(false);

        EventManager.Trigger("OnBreakpointChoiceSelected", 
            new GameEventData()
                .Set("TeamId", currentTeamId)
                .Set("Choice", choice));
    }
}
