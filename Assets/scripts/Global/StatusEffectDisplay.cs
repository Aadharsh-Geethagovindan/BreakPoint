using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class StatusEffectDisplay : MonoBehaviour
{
    public Transform buffPanel, debuffPanel;
    public GameObject iconPrefab;

    private Dictionary<StatusEffect, GameObject> activeIcons = new();

    public void UpdateIcons(List<StatusEffect> effects)
    {
        // Clear expired icons
        var expired = activeIcons.Keys.Except(effects).ToList();
        foreach (var expiredEffect in expired)
        {
            Destroy(activeIcons[expiredEffect]);
            activeIcons.Remove(expiredEffect);
        }

        /*
        // Add or update active icons
        foreach (var eff in effects)
        {
            if (!eff.ToDisplay) continue;

            if (!activeIcons.ContainsKey(eff))
            {
                var parent = eff.IsDebuff ? debuffPanel : buffPanel;
                var icon = Instantiate(iconPrefab, parent);
                var iconComp = icon.GetComponent<StatusEffectIcon>();
                iconComp.Initialize(StatusEffectIconMapper.GetIconFor(eff), eff.Duration);
                activeIcons[eff] = icon;
            }
            else
            {
                activeIcons[eff].GetComponent<StatusEffectIcon>().UpdateDuration(eff.Duration);
            }
        }
        */
    }
    
}
