using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
public class CharacterCardUI : MonoBehaviour
{
    public Image outlineImage;
    public Image targetedImage;
    public Button cardButton;            
    private GameCharacter characterRef;
    private Action<GameCharacter> onClickAction;

    public Slider shieldSlider; // Reference to the shield slider

    public StatusEffectDisplay statusEffectDisplay;

    public bool setRecent = true;
    public void SetCharacter(GameCharacter character)
    {
        characterRef = character;
    }

    public void SetTargetHighlight(bool enable)
    {
        if (outlineImage != null)
            outlineImage.enabled = enable;
    }

    public void SetTargetedHighlight(bool enable)
    {
        
        if (targetedImage != null)
            targetedImage.enabled = enable;
    }

    public void SetClickable(bool clickable, Action<GameCharacter> clickCallback)
    {
        onClickAction = clickable ? clickCallback : null;
        cardButton.interactable = clickable;

        // Clear old listeners
        cardButton.onClick.RemoveAllListeners();

        if (clickable)
        {
            cardButton.onClick.AddListener(() =>
            {
                if (characterRef != null && onClickAction != null)
                {
                    onClickAction.Invoke(characterRef);
                }
            });
        }
    }
    public void UpdateShieldBar(GameCharacter character)
    {
        int shield = character.Shield;

        if (shield > 0)
        {
            shieldSlider.gameObject.SetActive(true);
            shieldSlider.maxValue = 100; // Or dynamic max if needed
            shieldSlider.value = shield;
        }
        else
        {
            shieldSlider.gameObject.SetActive(false);
        }
    }

    public void SetOutlineDimmed(bool dimmed)
    {
        if (outlineImage == null) return;

        Color color = outlineImage.color;
        color.a = dimmed ? 0.3f : 1f;
        outlineImage.color = color;
    }

    public void SetCardDimmed(bool dimmed)
    {
        Image border = transform.Find("PortraitImage")?.GetComponent<Image>();
        if (border != null)
        {
            border.color = dimmed ? new Color(0.3f, 0.3f, 0.3f, 0.5f) : Color.white;
        }

        // Optional: disable interactivity
        GetComponent<Button>().interactable = !dimmed;
    }

    public void Shake(float duration = 0.3f, float magnitude = 10f)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float offsetY = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
    
    public void RefreshStatusEffects(GameCharacter character)
    {
        if (setRecent)
        {
            statusEffectDisplay.recentStatusImage = GameObject.Find("RecentStatusImage")?.GetComponent<Image>();
            //Debug.LogWarning("Set Recent");
            setRecent = false;
        }
        //Debug.Log("In refresh status");
        if (statusEffectDisplay != null)
        {
            //Debug.Log("Updating status effect display");
            statusEffectDisplay.UpdateStatusEffectDisplay(character);
        }
    }

}
