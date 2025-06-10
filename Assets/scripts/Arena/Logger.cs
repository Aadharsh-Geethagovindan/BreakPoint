using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

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
        GameObject entry = Instantiate(logEntryPrefab, logContainer.transform);
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
    }

    private void UpdateTransparency()
    {
        float[] transparencies = {1f, 0.75f, 0.5f, 0.35f, 0.2f};
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
            LogType.Status => Color.magenta,
            LogType.Passive => new Color(0.6f, 0.6f, 1f),
            LogType.Death => new Color(0.7f, 0f, 0f),
            LogType.Warning => new Color(0.7f, .67f, .08f),
            _ => Color.white
        };
    }
}
