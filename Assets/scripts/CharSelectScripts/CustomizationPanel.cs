using TMPro;
using UnityEngine;

public class CustomizationPanel : MonoBehaviour
{
    [SerializeField] TMP_InputField hpScaleInput;
    [SerializeField] TMP_InputField burndownStepsInput;
    [SerializeField] TMP_InputField percentScheduleInput;

    void OnEnable()
    {
        if (GameTuning.I == null) return;
        var d = GameTuning.I.data;
        hpScaleInput.text = d.hpScale.ToString("0.##");
        burndownStepsInput.text = d.burndownSteps.ToString();
        
    }

    public void OnApply()
    {
        if (GameTuning.I == null) return;
        var d = GameTuning.I.data;

        if (float.TryParse(hpScaleInput.text, out var hpScale))
            d.hpScale = Mathf.Clamp(hpScale, 0.1f, 5f);

        if (int.TryParse(burndownStepsInput.text, out var steps))
            d.burndownSteps = Mathf.Clamp(steps, 1, 20);

        if (percentScheduleInput != null)                                 
        {                                                                
            var parsed = ParseScheduleString(percentScheduleInput.text);  
            if (parsed != null && parsed.Length == 6)                     
                d.burndownPercentSchedule = parsed;                       
        }
    }

    public void OnResetDefaults()
    {
        if (GameTuning.I == null) return;
        GameTuning.I.data = new GameTuningData();
        OnEnable();
    }  

    private float[] ParseScheduleString(string text)
    {
        // Default fallback if parsing fails
        float[] fallback = new float[6] { 0.05f, 0.10f, 0.20f, 0.20f, 0.30f, 0.40f };
        if (string.IsNullOrWhiteSpace(text)) return fallback;

        var tokens = text.Split(',');
        var result = new float[6];
        int written = 0;

        foreach (var raw in tokens)
        {
            if (written >= 6) break;
            var s = raw.Trim();
            if (string.IsNullOrEmpty(s)) continue;

            if (float.TryParse(s, out var v))
            {
                // If user typed whole percents (e.g., 5 => 0.05)
                if (v > 1f) v = v / 100f;
                result[written++] = Mathf.Clamp01(v);
            }
        }

        if (written == 0) return fallback;

        // Pad remaining slots by repeating last valid value
        float last = result[Mathf.Max(0, written - 1)];
        for (int i = written; i < 6; i++) result[i] = last;

        return result;
    }
                                                
}
