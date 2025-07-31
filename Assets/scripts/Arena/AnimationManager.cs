using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationManager : MonoBehaviour
{
    private ActiveCharPanel activeCharPanel;
    [SerializeField] private Vector2 centerPosition = new Vector2(0, 0); // or wherever center is
    public static AnimationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void OnEnable()
    {
        EventManager.Subscribe("OnDamageDealt", HandleCardShake);
        activeCharPanel = Object.FindFirstObjectByType<ActiveCharPanel>();
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
    
    public IEnumerator AnimateCardRepositioning(List<GameCharacter> newOrder)
    {
        ActiveCharPanel activeCharPanel = Object.FindFirstObjectByType<ActiveCharPanel>();
        if (activeCharPanel == null)
        {
            Debug.LogError("ActiveCharPanel not found for animation.");
            yield break;
        }

        Vector2 centerPosition = new Vector2(0, 0);

        // Step 1: Collapse cards & HP bars to center
        foreach (var character in newOrder)
        {
            CharacterCardUI card = activeCharPanel.FindCardForCharacter(character);
            if (card != null)
            {
                StartCoroutine(card.MoveToPosition(centerPosition, 0.3f));

                if (card.HPBar != null)
                    StartCoroutine(card.HPBar.MoveToPosition(centerPosition + new Vector2(0, 92f), 0.3f));
            }
        }

        yield return new WaitForSeconds(0.4f);

        // Step 2: Reassign ClockPositions
        for (int i = 0; i < newOrder.Count; i++)
        {
            CharacterCardUI card = activeCharPanel.FindCardForCharacter(newOrder[i]);
            if (card != null)
            {
                float angleStep = 360f / newOrder.Count;
                float angle = -angleStep * i;
                Vector2 basePos = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * 280f;
                Vector2 finalCardPos = basePos + new Vector2(0, 60f);
                card.ClockPosition = finalCardPos;
            }
        }

        // Step 3: Spread to new ClockPositions
        foreach (var character in newOrder)
        {
            CharacterCardUI card = activeCharPanel.FindCardForCharacter(character);
            if (card != null)
            {
                StartCoroutine(card.MoveToPosition(card.ClockPosition, 0.4f));

                if (card.HPBar != null)
                    StartCoroutine(card.HPBar.MoveToPosition(card.ClockPosition + new Vector2(0, 92f), 0.4f));
            }
        }

        yield return new WaitForSeconds(0.5f);
    }






}
