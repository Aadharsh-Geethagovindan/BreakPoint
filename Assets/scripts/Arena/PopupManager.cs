using UnityEngine;

public enum PopupType
{
    Hit,
    Heal,
    Buff,
    Miss,
    Shield,
    Immune
}

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;

    [Header("Popup Prefabs")]
    public GameObject hitPopupPrefab;
    public GameObject healPopupPrefab;
    public GameObject buffPopupPrefab;
    public GameObject shieldPopupPrefab;
    public GameObject missPopupPrefab;
    public GameObject immunePopupPrefab;
    public Transform centerAnchor; // Drag your PopupAnchor object here


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void OnEnable()
    {
        EventManager.Subscribe("OnDamageDealt", ShowDamagePopup);
        EventManager.Subscribe("OnHealed", ShowHealPopup);
        EventManager.Subscribe("OnShielded", ShowShieldPopup);
        EventManager.Subscribe("OnMiss", ShowMissPopup);
        EventManager.Subscribe("OnBuffApplied", ShowBuffPopup);
        EventManager.Subscribe("OnImmunityTriggered", ShowImmunePopup);
    }

    void OnDisable()
    {
        EventManager.Unsubscribe("OnDamageDealt", ShowDamagePopup);
        EventManager.Unsubscribe("OnHealed", ShowHealPopup);
        EventManager.Unsubscribe("OnShielded", ShowShieldPopup);
        EventManager.Unsubscribe("OnMiss", ShowMissPopup);
        EventManager.Unsubscribe("OnBuffApplied", ShowBuffPopup);
        EventManager.Unsubscribe("OnImmunityTriggered", ShowImmunePopup);
    }
    public void ShowPopup(PopupType type)
    {
        //Debug.Log($"PopupManager ShowPopup on instance {GetInstanceID()}, this==null? {this == null}, centerAnchor null? {centerAnchor == null}", this);

        if (centerAnchor == null)
        {
            Debug.LogWarning("PopupManager: Center anchor not assigned.");
            return;
        }

        GameObject prefab = GetPopupPrefab(type);
        if (prefab == null)
        {
            Debug.LogWarning($"PopupManager: No prefab assigned for {type}");
            return;
        }

        GameObject popup = Instantiate(prefab, centerAnchor.position, Quaternion.identity, centerAnchor);
        //Debug.Log("Showing popup");
        Destroy(popup, 2f);
    }


    private GameObject GetPopupPrefab(PopupType type)
    {
        switch (type)
        {
            case PopupType.Hit: return hitPopupPrefab;
            case PopupType.Heal: return healPopupPrefab;
            case PopupType.Buff: return buffPopupPrefab;
            case PopupType.Miss: return missPopupPrefab;
            case PopupType.Shield: return shieldPopupPrefab;
            case PopupType.Immune: return immunePopupPrefab;
            default: return null;
        }
    }

    private void ShowDamagePopup(object eventData)
    {
        var evt = eventData as GameEventData;
        var target = evt?.Get<GameCharacter>("Target");

        if (target != null)
            ShowPopup(PopupType.Hit);
    }

    private void ShowHealPopup(object eventData)
    {
        ShowPopup(PopupType.Heal);
    }
    private void ShowMissPopup(object eventData)
    {
        ShowPopup(PopupType.Miss);
    }

    private void ShowShieldPopup(object eventData)
    {
        ShowPopup(PopupType.Shield);
    }

    private void ShowBuffPopup(object eventData)
    {
        ShowPopup(PopupType.Buff);
    }

    private void ShowImmunePopup(object eventData)
    {
        ShowPopup(PopupType.Immune);
    }
}
