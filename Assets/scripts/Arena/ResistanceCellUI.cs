using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResistanceCellUI : MonoBehaviour
{
    [SerializeField] public Image icon;     // assign in Inspector
    [SerializeField] public TMP_Text value; // assign in Inspector

    // Map abs(res) to alpha (0.6 → 1.0 when |res| goes 0 → 0.5+)
    private float AlphaFor(float res)
    {
        float t = Mathf.Clamp01(Mathf.Abs(res) / 0.5f);
        return Mathf.Lerp(0.6f, 1f, t);
    }

    private Color ColorFor(float res)
    {
        if (res > 0.0001f) return new Color32(124, 255, 119, 255);   // green
        if (res < -0.0001f) return new Color32(255, 106, 106, 255);  // red
        return new Color32(204, 204, 204, 255);                       // neutral
    }

    private static string FormatPercent(float res)
    {
        int pct = Mathf.RoundToInt(res * 100f);
        return (pct > 0 ? $"+{pct}%" : $"{pct}%");
    }

    public void SetValue(float res)
    {
        if (value != null)
        {
            value.text = FormatPercent(res);
            var c = ColorFor(res);
            c.a = AlphaFor(res);
            value.color = c;
        }

        if (icon != null)
        {
            var ic = icon.color;
            ic.a = AlphaFor(res);
            icon.color = ic; // keep sprite tint, just adjust alpha
        }
    }
}
