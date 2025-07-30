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

    void OnEnable()
    {
        EventManager.Subscribe("OnAbilityUsed", HandleAbilityUsed);
        EventManager.Subscribe("OnDamageDealt", HandleDamageDealt);
        EventManager.Subscribe("OnMiss", HandleMissed);
        EventManager.Subscribe("OnHealApplied", HandleHealed);
        EventManager.Subscribe("OnShieldApplied", HandleShielded);
        EventManager.Subscribe("OnStatusApplied", HandleStatusApplied);
        EventManager.Subscribe("OnCharacterDied", HandleCharacterDied);
        EventManager.Subscribe("OnCharacterRevived", HandleCharacterRevived);
        EventManager.Subscribe("OnStatusEffectExpired", HandleStatusExpired);
        EventManager.Subscribe("OnPassiveTriggered", HandlePassiveTriggered);
        EventManager.Subscribe("OnChargeIncreased", HandleChargeIncreased);
        EventManager.Subscribe("OnChargeDecreased", HandleChargeDecreased);
        EventManager.Subscribe("OnBurnDownDMG", HandleBurnDownDMG);

    }

    void OnDisable()
    {
        EventManager.Unsubscribe("OnAbilityUsed", HandleAbilityUsed);
        EventManager.Unsubscribe("OnDamageDealt", HandleDamageDealt);
        EventManager.Unsubscribe("OnHealApplied", HandleHealed);
        EventManager.Unsubscribe("OnShieldApplied", HandleShielded);
        EventManager.Unsubscribe("OnStatusApplied", HandleStatusApplied);
        EventManager.Unsubscribe("OnCharacterDied", HandleCharacterDied);
        EventManager.Unsubscribe("OnCharacterRevived", HandleCharacterRevived);
        EventManager.Unsubscribe("OnStatusEffectExpired", HandleStatusExpired);
        EventManager.Unsubscribe("OnPassiveTriggered", HandlePassiveTriggered);
        EventManager.Unsubscribe("OnChargeIncreased", HandleChargeIncreased);
        EventManager.Unsubscribe("OnChargeDecreased", HandleChargeDecreased);
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
            LogType.Info => new Color(1f, .67f, 0f),
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

    //*******************************************************************************************************************
    // Broadcast UI Methods
    //*******************************************************************************************************************
    void HandleAbilityUsed(object data)
    {
        GameEventData evt = data as GameEventData;
        if (evt == null) return;

        GameCharacter user = evt.Get<GameCharacter>("User");
        Ability ability = evt.Get<Ability>("Ability");
        List<GameCharacter> targets = evt.Get<List<GameCharacter>>("Targets");

        string log = $"{user.Name} used {ability.Name} on ";
        log += string.Join(", ", targets.ConvertAll(t => t.Name));

        PostLog(log, LogType.Info);
    }
    private void HandleDamageDealt(object eventData)
    {
        
        var evt = eventData as GameEventData;
        if (evt == null) return;

        var source = evt.Get<GameCharacter>("Source");
        var target = evt.Get<GameCharacter>("Target");
        var amount = evt.Get<int>("Amount");

        if (source != null && target != null)
        {
            PostLog($"{target.Name} took {amount} damage from {source.Name}", LogType.Damage);
        }
    }
    private void HandleMissed(object eventData)
    {
        
        var evt = eventData as GameEventData;
        if (evt == null) return;

        var source = evt.Get<GameCharacter>("Source");
        var target = evt.Get<GameCharacter>("Target");
        var ability = evt.Get<Ability>("Ability");

        if (source != null && target != null)
        {
            PostLog($"{source.Name} missed {target.Name} with {ability.Name}", LogType.Damage);
        }
    }
   private void HandleHealed(object eventData)
    {
        var evt = eventData as GameEventData;
        if (evt == null) return;

        var source = evt.Get<GameCharacter>("Source");
        var target = evt.Get<GameCharacter>("Target");
        var amount = evt.Get<int>("Amount");

        if (source != null && target != null)
        {
            PostLog($"{target.Name} was healed for {amount} by {source.Name}", LogType.Heal);
        }
    }

    private void HandleShielded(object eventData)
    {
        var evt = eventData as GameEventData;
        if (evt == null) return;

        var source = evt.Get<GameCharacter>("Source");
        var target = evt.Get<GameCharacter>("Target");
        var amount = evt.Get<int>("Amount");

        if (source != null && target != null)
        {
            PostLog($"{target.Name} gained a shield of {amount} from {source.Name}", LogType.Shield);
        }
    }

    private void HandleStatusApplied(object eventData)
    {
        var evt = eventData as GameEventData;
        if (evt == null) return;

        var source = evt.Get<GameCharacter>("Source");
        var target = evt.Get<GameCharacter>("Target");
        var effect = evt.Get<StatusEffect>("Effect");

        if (source != null && target != null && effect != null)
        {
            PostLog($"{target.Name} gained {effect.Name} from {source.Name} for {effect.Duration} turn(s)", LogType.Status);
        }
    }

    private void HandleBurnDownDMG(object eventData)
    {
        var evt = eventData as GameEventData;
        if (evt == null) return;

        var target = evt.Get<GameCharacter>("Target");
        var damage = evt.Get<int>("Damage");

        if (target != null)
        {
            PostLog($"{target.Name} suffers {damage} burndown damage!",LogType.Status);
        }
    }


    private void HandleCharacterDied(object eventData)
    {
        var evt = eventData as GameEventData;
        if (evt == null) return;

        var character = evt.Get<GameCharacter>("Character");

        if (character != null)
        {
            PostLog($"{character.Name} has died!", LogType.Death);
        }
    }

    private void HandleCharacterRevived(object eventData)
    {
        var evt = eventData as GameEventData;
        if (evt == null) return;

        var character = evt.Get<GameCharacter>("Character");

        if (character != null)
        {
            PostLog($"{character.Name} was revived!", LogType.Heal);
        }
    }

    private void HandleStatusExpired(object eventData)
    {
        var evt = eventData as GameEventData;
        if (evt == null) return;

        var target = evt.Get<GameCharacter>("Target");
        var effect = evt.Get<StatusEffect>("Effect");

        if (target != null && effect != null)
        {
            PostLog($"{effect.Name} expired on {target.Name}", LogType.Status);
        }
    }

    private void HandlePassiveTriggered(object eventData)
    {
        var evt = eventData as GameEventData;
        if (evt == null) return;

        var source = evt.Get<GameCharacter>("Source");
        var description = evt.Get<string>("Description");

        if (source != null && !string.IsNullOrEmpty(description))
        {
            PostLog($"{source.Name} passive: {description}", LogType.Passive);
        }
    }

    private void HandleChargeIncreased(object eventData)
    {
        var evt = eventData as GameEventData;
        if (evt == null) return;

        var character = evt.Get<GameCharacter>("Character");
        var amount = evt.Get<int>("Amount");

        if (character != null)
        {
            PostLog($"{character.Name} gained {amount} signature charge", LogType.Buff);
        }
    }

    private void HandleChargeDecreased(object eventData)
    {
        var evt = eventData as GameEventData;
        if (evt == null) return;

        var character = evt.Get<GameCharacter>("Character");
        var amount = evt.Get<int>("Amount");

        if (character != null)
        {
            PostLog($"{character.Name} lost {amount} signature charge", LogType.Debuff);
        }
    }






}
