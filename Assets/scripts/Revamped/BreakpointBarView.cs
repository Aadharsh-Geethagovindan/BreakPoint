using UnityEngine;
using UnityEngine.UI;

public class BreakpointBarView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform fillLeft;
    [SerializeField] private RectTransform fillRight;

    [Header("Config")]
    [SerializeField] private float maxPixelWidth = 430f; // set to half the total bar width
    [SerializeField] private float lerpSpeed = 8f;

    [SerializeField] private float celebrateHold = 0.6f;   // NEW: time to hold at edge
    [SerializeField] private float pulseScale = 1.08f;      // NEW: slight width overshoot for pulse
    [SerializeField] private float pulseTime = 0.18f;       // NEW: pulse duration (each way)

    private bool celebrating = false;                       // NEW
    private Coroutine celebrateCo;

    private float targetValue; // -1..+1 normalized
    private float currentValue;

    private void OnEnable()
    {
        EventManager.Subscribe("OnBreakpointUpdated", HandleBreakpointUpdate);
        EventManager.Subscribe("OnBreakpointTriggered", HandleBreakpointTriggered);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe("OnBreakpointUpdated", HandleBreakpointUpdate);
        EventManager.Unsubscribe("OnBreakpointTriggered", HandleBreakpointTriggered);
    }

    private void Update()
    {
        if (!celebrating)                                   // NEW
        {
            // Smooth motion
            currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * lerpSpeed);
            UpdateVisual(currentValue);
        }
    }

    private void HandleBreakpointUpdate(object data)
    {
        if (celebrating) return;
        if (data is GameEventData evt)
        {
            float value = evt.Get<float>("Value"); // current bar value
            float cap = evt.Get<float>("Cap");   // max magnitude

            targetValue = Mathf.Clamp(value / cap, -1f, 1f);
        }
    }
    
    private void HandleBreakpointTriggered(object data)     // NEW
    {
        var evt = data as GameEventData;
        if (evt == null) return;

        int teamId = evt.Get<int>("TeamId");
        // Optional: string choice = evt.Get<string>("Choice");

        if (celebrateCo != null) StopCoroutine(celebrateCo);
        celebrateCo = StartCoroutine(Celebrate(teamId));
    }

    private System.Collections.IEnumerator Celebrate(int teamId) // NEW
    {
        celebrating = true;

        // Snap to the winning edge visually
        currentValue = targetValue = (teamId == 1) ? 1f : -1f;
        UpdateVisual(currentValue);

        // Pulse effect: briefly overshoot width then return
        float baseWidth = Mathf.Abs(currentValue) * maxPixelWidth;
        float overWidth = baseWidth * pulseScale;

        // We’ll drive width manually during the pulse
        // Grow
        float t = 0f;
        while (t < pulseTime)
        {
            t += Time.deltaTime;
            float k = t / pulseTime;
            float w = Mathf.Lerp(baseWidth, overWidth, k);
            ApplyManualWidth(currentValue, w);
            yield return null;
        }
        // Shrink back
        t = 0f;
        while (t < pulseTime)
        {
            t += Time.deltaTime;
            float k = t / pulseTime;
            float w = Mathf.Lerp(overWidth, baseWidth, k);
            ApplyManualWidth(currentValue, w);
            yield return null;
        }

        // Hold at edge so the player sees it
        yield return new WaitForSeconds(celebrateHold);

        // Exit celebration: allow normal updates again and head back to center
        celebrating = false;
        targetValue = 0f; // UI will lerp back; manager also sends 0 soon after
    }

    private void ApplyManualWidth(float normalized, float width) // NEW
    {
        if (normalized > 0f)
        {
            fillRight.sizeDelta = new Vector2(width, fillRight.sizeDelta.y);
            fillLeft.sizeDelta  = new Vector2(0,     fillLeft.sizeDelta.y);
        }
        else
        {
            fillLeft.sizeDelta  = new Vector2(width, fillLeft.sizeDelta.y);
            fillRight.sizeDelta = new Vector2(0,     fillRight.sizeDelta.y);
        }
    }

    private void UpdateVisual(float t)
    {
        float width = Mathf.Abs(t) * maxPixelWidth;

        if (t > 0)
        {
            fillRight.sizeDelta = new Vector2(width, fillRight.sizeDelta.y);
            fillLeft.sizeDelta = new Vector2(0, fillLeft.sizeDelta.y);
        }
        else if (t < 0)
        {
            fillLeft.sizeDelta = new Vector2(width, fillLeft.sizeDelta.y);
            fillRight.sizeDelta = new Vector2(0, fillRight.sizeDelta.y);
        }
        else
        {
            fillLeft.sizeDelta = new Vector2(0, fillLeft.sizeDelta.y);
            fillRight.sizeDelta = new Vector2(0, fillRight.sizeDelta.y);
        }
    }
}
