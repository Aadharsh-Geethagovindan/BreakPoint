using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIPulseLine2 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image baseLine;
    [SerializeField] private Image pulseOverlay;

    [Header("Pulse Settings")]
    [SerializeField] private float pulseDuration = 1.5f;
    [SerializeField] private float waitBetween = 2.0f;
    [SerializeField] private float pulseWidth = 0.25f;   // portion of line lit at once (0–1)
    [SerializeField] private bool horizontal = true;

    private Material mat;
    private float timer;

    void Awake()
    {
        if (pulseOverlay != null)
        {
            // Duplicate its material so each line animates independently
            mat = new Material(pulseOverlay.material);
            pulseOverlay.material = mat;
        }
    }

    void Update()
    {
        if (mat == null) return;

        timer += Time.deltaTime;
        float cycle = (pulseDuration + waitBetween);
        float t = Mathf.Repeat(timer, cycle);

        float progress = Mathf.Clamp01(t / pulseDuration); // 0→1 only during active pulse
        float offset = Mathf.Lerp(-pulseWidth, 1f, progress);

        // Animate UV offset across the line
        if (horizontal)
            mat.SetTextureOffset("_MainTex", new Vector2(offset, 0));
        else
            mat.SetTextureOffset("_MainTex", new Vector2(0, offset));

        // Fade overlay in/out at ends
        float fade = Mathf.SmoothStep(0, 1, Mathf.PingPong(progress * 2, 1));
        var c = pulseOverlay.color;
        c.a = fade;
        pulseOverlay.color = c;
    }
}