using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class StatusEffectChipUI : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text duration;
    [SerializeField] private Image icon;          // optional
    [SerializeField] private Outline outline;     

    public void Initialize(StatusEffect effect)
    {
        title.text = effect.Name;
        duration.text = effect.Duration > 0 ? effect.Duration.ToString() : "∞";

        if (outline != null)
            outline.effectColor = effect.IsDebuff ? new Color(1f, 0.3f, 0.3f, 1f) 
                                                : new Color(0.3f, 1f, 0.3f, 1f);
    }

    public void Refresh(StatusEffect effect)
    {
        duration.text = effect.Duration > 0 ? effect.Duration.ToString() : "∞";
    }
}
