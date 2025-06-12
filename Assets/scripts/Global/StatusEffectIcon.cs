using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class StatusEffectIcon : MonoBehaviour
{
    public TMP_Text durationText;
    public Image iconImage;

    public void Initialize(Sprite sprite, int duration)
    {
        iconImage.sprite = sprite;
        UpdateDuration(duration);
    }

    public void UpdateDuration(int duration)
    {
        durationText.text = duration.ToString();
    }
}
