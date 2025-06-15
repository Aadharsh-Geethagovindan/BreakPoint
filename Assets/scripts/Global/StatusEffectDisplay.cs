using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
public class StatusEffectDisplay : MonoBehaviour
{
    public Transform buffPanel, debuffPanel;
    public GameObject iconPrefab;

    [SerializeField] public Image recentStatusImage;

    private Dictionary<StatusEffect, GameObject> activeIcons = new();
    
    

    public void UpdateStatusEffectDisplay(GameCharacter character)
    {
        // Clear previous icons
        foreach (Transform child in buffPanel) Destroy(child.gameObject);
        foreach (Transform child in debuffPanel) Destroy(child.gameObject);
        //Debug.Log("In Update Status Effect Display");
        foreach (StatusEffect effect in character.StatusEffects)
        {
            if (!effect.ToDisplay) continue;

            string iconName = effect.GetIconName();
            if (string.IsNullOrEmpty(iconName)) continue;

            Sprite icon = Resources.Load<Sprite>($"EffectIcons/{iconName}");
            if (icon == null)
            {
                Debug.LogWarning($"Missing icon: {iconName}");
                continue;
            }
            Debug.Log($"Instantiating {iconName}");

            GameObject iconObj = Instantiate(iconPrefab, effect.IsDebuff ? debuffPanel : buffPanel);
            Image iconImage = iconObj.GetComponent<Image>(); // not child
            TextMeshProUGUI durText = iconObj.GetComponentInChildren<TextMeshProUGUI>();
            if (iconImage != null && durText != null)
            {
                iconImage.sprite = icon;
                durText.text = effect.Duration.ToString();
            }
            else
                Debug.LogError("No Image found in status icon prefab.");

            if (recentStatusImage != null)
            {
                recentStatusImage.sprite = icon;
                recentStatusImage.color = Color.white; // Ensure it's visible
            }

        }
    }

   


}
