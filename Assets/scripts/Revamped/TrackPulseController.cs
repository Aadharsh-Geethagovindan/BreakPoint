using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Essence = DamageType;

public class TrackPulseController : MonoBehaviour
{
    [Header("Track Info")]
    public int teamId = 1;
    public Essence essence;

    [Header("Glow Settings")]
    public Image glowOverlay;
    public float pulseSpeed = 2.5f;
    public float pulseIntensity = 1f;   // max alpha
    public Color essenceColor = Color.white;

    private Coroutine pulseRoutine;

    private void Awake()
    {
        if (glowOverlay == null)
            glowOverlay = transform.Find("GlowOverlay")?.GetComponent<Image>();
    }

    private void OnEnable()
    {
        EventManager.Subscribe("OnTrackMarkAdded", HandleTrackMarkAdded);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe("OnTrackMarkAdded", HandleTrackMarkAdded);
    }

    private void HandleTrackMarkAdded(object data)
    {
        if (data is not GameEventData evt) return;

        int evtTeam = evt.Get<int>("TeamId");
        Essence evtEssence = evt.Get<Essence>("Essence");
        int current = evt.Get<int>("CurrentMarks");
        int threshold = evt.Get<int>("Threshold");

        // only react to this track
        if (evtTeam != teamId || evtEssence != essence)
            return;

        // start pulse when filled
        if (current >= threshold)
        {
            if (pulseRoutine != null)
                StopCoroutine(pulseRoutine);
            pulseRoutine = StartCoroutine(PulseGlow());
        }
        // stop pulse when reset
        else if (current == 0 && pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
            if (glowOverlay != null)
                glowOverlay.color = new Color(essenceColor.r, essenceColor.g, essenceColor.b, 0f);
        }
    }

    private IEnumerator PulseGlow()
    {
        if (glowOverlay == null)
            yield break;
        Color baseColor = glowOverlay.color;
        float timer = 0f;
        while (true)
        {
            timer += Time.deltaTime * pulseSpeed;
            float alpha = (Mathf.Sin(timer) * 0.5f + 0.5f) * pulseIntensity;
            glowOverlay.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            yield return null;
        }
    }
}
