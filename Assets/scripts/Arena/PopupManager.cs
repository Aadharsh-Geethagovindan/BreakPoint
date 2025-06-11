using UnityEngine;

public enum PopupType
{
    Hit,
    Heal,
    Buff,
    Miss,
    Shield
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

    public void ShowPopup(PopupType type)
    {
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
            default: return null;
        }
    }
}
