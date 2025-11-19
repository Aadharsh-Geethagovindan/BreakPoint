using UnityEngine;
using UnityEngine.UI;

public class PulseLine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image pulseOverlay;

    [Header("Pulse Settings")]
    [SerializeField] private bool horizontal = true;
    [SerializeField] private bool flipDirection = false;

    [Header("Timing Ranges (seconds)")]
    [SerializeField] private float minPulseDuration = 0.8f;
    [SerializeField] private float maxPulseDuration = 1.5f;

    [SerializeField] private float minWaitBetween = 0.3f;
    [SerializeField] private float maxWaitBetween = 1.2f;

    [Header("Motion")]
    [SerializeField] private float pulseWidth = 0.2f; // how far "off-screen" the pulse starts
    [SerializeField] private bool randomizeOnStart = true;

    [Header("Sequencing")]
    [SerializeField] private PulseLine nextLine;
    [SerializeField] private bool chainTrigger = false;
    [SerializeField] private bool isLeader = true;
    [SerializeField] private float timeDelay = 2f;    

    [Header("Color Shift")]
    [SerializeField] private bool enableColorShift = false;
    [SerializeField] private Color colorA = Color.cyan;
    [SerializeField] private Color colorB = Color.white;
    [SerializeField, Range(0.1f, 5f)] private float colorLerpSpeed = 1f;
    private Material mat;
    private float pulseDuration;
    private float waitBetween;
    private float timer;
    private float cycle;
    private bool isWaiting;
    private bool activePulse = false;

    private void Awake()
    {
        if (pulseOverlay != null)
        {
            // Always use a material instance to avoid affecting shared material
            mat = Instantiate(pulseOverlay.material);
            pulseOverlay.material = mat;
        }
    }

    private void OnEnable()
    {
        if (randomizeOnStart)
            RandomizeCycle();
    }

    private void RandomizeCycle()
    {
        pulseDuration = Random.Range(minPulseDuration, maxPulseDuration);
        waitBetween   = Random.Range(minWaitBetween, maxWaitBetween);
        cycle         = pulseDuration + waitBetween;
        timer         = Random.Range(0f, cycle); // staggered start time
    }

    private void Update()
    {
        if (!isLeader && !activePulse) return;
        if (mat == null || pulseOverlay == null) return;

        timer += Time.deltaTime;

        // Reset and randomize next cycle
        if (timer >= cycle)
        {
            activePulse = false;

            // Notify next line if chaining is enabled
            if (chainTrigger && nextLine != null)
            {
                nextLine.TriggerPulse();
            }

            if (isLeader) // leaders randomize and loop
            {
                RandomizeCycle();
                timer = 0f;
                activePulse = true;
            }
        }
        

        float progress = Mathf.Clamp01(timer / pulseDuration); // 0→1 only during active pulse
        float offset = Mathf.Lerp(-pulseWidth, 1f, progress);

        if (flipDirection)
            offset = -offset;

        // Animate UV offset
        if (horizontal)
            mat.SetTextureOffset("_MainTex", new Vector2(offset, 0));
        else
            mat.SetTextureOffset("_MainTex", new Vector2(0, offset));

        // Fade overlay in/out at ends
        float fade = Mathf.SmoothStep(0, 1, Mathf.PingPong(progress * 2, 1));
        var c = pulseOverlay.color;
        c.a = fade;
        if (enableColorShift)
        {
            // Lerp between the two colors using a smooth ping-pong
            float colorT = Mathf.PingPong(Time.time * colorLerpSpeed, 1f);
            Color lerped = Color.Lerp(colorA, colorB, colorT);
            lerped.a = pulseOverlay.color.a; // preserve current alpha
            pulseOverlay.color = lerped;
        }
        else
        {
            pulseOverlay.color = c;
        }
    }
    
    public void TriggerPulse()
    {
        activePulse = true;
        timer = timeDelay;
        pulseDuration = Random.Range(minPulseDuration, maxPulseDuration);
        waitBetween = 0f;
        cycle = pulseDuration;
    }

}
