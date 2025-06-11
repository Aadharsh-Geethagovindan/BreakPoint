using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
public enum LogType
{
    Damage,
    Heal,
    Shield,
    Buff,
    Debuff,
    Status,
    Miss,
    Passive,
    Death,
    Info,
    Warning
}
public class Logger : MonoBehaviour
{
    public static Logger Instance;
    [SerializeField] private GameObject logContainer;
    [SerializeField] private GameObject logEntryPrefab;
    [SerializeField] private int maxLogs = 20;

    private Queue<GameObject> logQueue = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public void PostLog(string message, LogType type)
    {
        //GameObject entry = Instantiate(logEntryPrefab, logContainer.transform);
        GameObject entry = Instantiate(logEntryPrefab);
        entry.transform.SetParent(logContainer.transform, false);
        entry.transform.SetAsFirstSibling();
        var text = entry.GetComponent<TMP_Text>();
        text.text = message;
        text.color = GetColorByType(type);

        logQueue.Enqueue(entry);
        UpdateTransparency();

        if (logQueue.Count > maxLogs)
        {
            GameObject old = logQueue.Dequeue();
            Destroy(old);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)logContainer.transform); // Force Unity to rebuild the UI after every new message so its atcking properly
        //scroll to top
        Canvas.ForceUpdateCanvases();
        var scrollRect = GetComponentInParent<ScrollRect>();
        scrollRect.verticalNormalizedPosition = 1f; // 1 = top

    }

    private void UpdateTransparency()
    {
        float[] transparencies = { 1f, 0.8f, 0.6f, 0.45f, 0.3f };
        int i = 0;

        foreach (var entry in logQueue.Reverse())
        {
            var color = entry.GetComponent<TMP_Text>().color;
            float alpha = i < transparencies.Length ? transparencies[i] : transparencies[^1];
            entry.GetComponent<TMP_Text>().color = new Color(color.r, color.g, color.b, alpha);
            i++;
        }
    }

    private Color GetColorByType(LogType type)
    {
        return type switch
        {
            LogType.Damage => Color.red,
            LogType.Heal => Color.green,
            LogType.Shield => Color.cyan,
            LogType.Buff => Color.yellow,
            LogType.Debuff => new Color(1f, 0.4f, 0f),
            LogType.Miss => Color.gray,
            LogType.Info => new Color(1f,.67f,0f),
            LogType.Status => Color.magenta,
            LogType.Passive => new Color(0.6f, 0.6f, 1f),
            LogType.Death => new Color(0.7f, 0f, 0f),
            LogType.Warning => new Color(0.7f, .67f, .08f),
            _ => Color.white
        };
    }
    
    public void SetAllTransparency(float alpha)
    {
        foreach (var entry in logQueue)
        {
            var text = entry.GetComponent<TMP_Text>();
            var color = text.color;
            text.color = new Color(color.r, color.g, color.b, alpha);
        }
    }

    public void SetDynamicTransparency()
    {
        UpdateTransparency(); // Restore dynamic fade
    }

}
