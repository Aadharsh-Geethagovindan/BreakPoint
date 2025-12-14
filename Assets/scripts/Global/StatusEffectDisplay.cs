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
            //Debug.Log($"Instantiating {iconName}");

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

   public void UpdateStatusEffectDisplayFromSnapshot(List<StatusEffectState> statusStates)
    {
        // Clear previous icons
        foreach (Transform child in buffPanel) Destroy(child.gameObject);
        foreach (Transform child in debuffPanel) Destroy(child.gameObject);

        if (statusStates == null || statusStates.Count == 0)
            return;

        foreach (var se in statusStates)
        {
            // Snapshot stores Type as string (ex: "Stun", "Electroshock", etc.)
            if (string.IsNullOrEmpty(se.Type))
                continue;

            // Your existing UI expects Resources/EffectIcons/<iconName>
            // So we need a mapping from snapshot type -> icon file name.
            // If your icon filenames match the type string exactly, this works immediately.
            string iconName = se.Type;

            Sprite icon = Resources.Load<Sprite>($"EffectIcons/{iconName}");
            if (icon == null)
            {
                Debug.LogWarning($"Missing icon: {iconName}");
                continue;
            }

            // Decide buff/debuff.
            // Snapshot doesn't currently include IsDebuff, so we infer it.
            // If you want this to be correct, add IsDebuff to StatusEffectState later.
            bool isDebuff = IsDebuffFromTypeString(se.Type);

            GameObject iconObj = Instantiate(iconPrefab, isDebuff ? debuffPanel : buffPanel);
            Image iconImage = iconObj.GetComponent<Image>();
            TextMeshProUGUI durText = iconObj.GetComponentInChildren<TextMeshProUGUI>();

            if (iconImage != null && durText != null)
            {
                iconImage.sprite = icon;
                durText.text = se.RemainingTurns.ToString();
            }
            else
            {
                Debug.LogError("No Image found in status icon prefab.");
            }

            if (recentStatusImage != null)
            {
                recentStatusImage.sprite = icon;
                recentStatusImage.color = Color.white;
            }
        }
    }

    // Temporary inference. Replace with explicit se.IsDebuff later for correctness.
    private bool IsDebuffFromTypeString(string type)
    {
        // Treat common negative effects as debuffs.
        // Add to this list as needed.
        switch (type)
        {
            case "Stun":
            case "Weakened":
            case "Electroshock":
            case "Poison":
            case "Burn":
            case "Bleed":
                return true;
            default:
                return false;
        }
    }


}
