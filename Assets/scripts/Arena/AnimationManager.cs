using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.Subscribe("OnDamageDealt", HandleCardShake);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe("OnDamageDealt", HandleCardShake);
    }

    private void HandleCardShake(object eventData)
    {
        var evt = eventData as GameEventData;
        if (evt == null) return;

        var target = evt.Get<GameCharacter>("Target");
        
        ActiveCharPanel panel = UnityEngine.Object.FindFirstObjectByType<ActiveCharPanel>();
        CharacterCardUI card = panel?.FindCardForCharacter(target);
        
            if (card != null)
        {
            card.Shake(); // 🌀 Trigger the shake animation
        }
        
    }


}
