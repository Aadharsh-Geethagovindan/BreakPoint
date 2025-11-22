using UnityEngine;

public class AbilityResultDebugger : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.Subscribe("OnExecuteAbility", OnAbilityResolved);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe("OnExecuteAbility", OnAbilityResolved);
    }

    private void OnAbilityResolved(object param)
    {
        if (param is not AbilityResult result)
        {
            UnityEngine.Debug.LogWarning(
                $"OnAbilityResolved received wrong type: {param?.GetType().Name ?? "null"}");
            return;
        }

        var msg = $"[AbilityResult] Caster {result.CasterId}, Ability {result.AbilityType}, targets {result.Targets.Count}";
        foreach (var tr in result.Targets)
        {
            msg += $"\n  Target {tr.TargetId}: Hit={tr.Hit}, Damage={tr.Damage}, HPAfter={tr.HPAfter}, Effects={tr.AppliedEffects.Count}";
        }

        UnityEngine.Debug.Log(msg);
    }
}
