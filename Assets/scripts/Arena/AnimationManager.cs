using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class AnimationManager : MonoBehaviour
{
    [Header("Scene refs")]
    private ActiveCharPanel activeCharPanel;
    [SerializeField] private Vector2 centerPosition = new Vector2(0, 0); // or wherever center is
    [SerializeField] private RectTransform projectileParent;

    [Header("Projectile visuals")]
    [SerializeField] private GameObject projectileUIPrefab;      // a simple GameObject with Image component
    [SerializeField] private Sprite elementalSprite;
    [SerializeField] private Sprite arcaneSprite;
    [SerializeField] private Sprite forceSprite;
    [SerializeField] private Sprite corruptSprite;
    [SerializeField] private Sprite trueSprite;

    [Header("Tuning")]
    [SerializeField] private float pixelsPerSecond = 1600f;      // speed
    [SerializeField] private float arcPixels = 120f;             // height of the arc (0 = straight line)
    [SerializeField] private float dodgeImpactLead = 0.20f;   // dodge starts at ~80% of flight
    [SerializeField] private float dodgeOffsetPixels = 200f;   // how far the card sidesteps
    [SerializeField] private float dodgeOutDuration = 0.15f;  // step out
    [SerializeField] private float dodgeReturnDuration = 0.15f; // step back

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
        //EventManager.Subscribe("OnDamageDealt", HandleDamageDealt);
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

    private void HandleDamageDealt(object data)
    {
        var evt = data as GameEventData;
        if (evt == null) return;

        var source = evt.Get<GameCharacter>("Source");
        var target = evt.Get<GameCharacter>("Target");
        var type = evt.Get<DamageType>("Type");

        if (source == null || target == null) return;

        var srcCard = activeCharPanel.FindCardForCharacter(source);
        var tgtCard = activeCharPanel.FindCardForCharacter(target);
        if (srcCard == null || tgtCard == null) return;

        var sprite = GetSpriteFor(type);
        StartCoroutine(FireProjectile(srcCard.GetComponent<RectTransform>(),
                                      tgtCard.GetComponent<RectTransform>(),
                                      sprite));
    }

    private Sprite GetSpriteFor(DamageType type)
    {
        switch (type)
        {
            case DamageType.Elemental: return elementalSprite;
            case DamageType.Arcane: return arcaneSprite;
            case DamageType.Force: return forceSprite;
            case DamageType.Corrupt: return corruptSprite;
            case DamageType.True: return trueSprite ? trueSprite : forceSprite; // fallback
            default: return forceSprite;
        }
    }

    private IEnumerator FireProjectile(RectTransform from, RectTransform to, Sprite sprite)
    {
        if (projectileUIPrefab == null || projectileParent == null) yield break;

        // Spawn projectile UI
        var go = Instantiate(projectileUIPrefab, projectileParent);
        var img = go.GetComponent<Image>();
        var proj = go.GetComponent<RectTransform>();
        if (img != null) img.sprite = sprite;

        // Start/End in the same anchored space as cards
        Vector2 start = from.anchoredPosition;
        Vector2 end = to.anchoredPosition;
        proj.anchoredPosition = start;

        // Travel time based on distance and configured speed
        float dist = Vector2.Distance(start, end);
        float duration = Mathf.Max(0.05f, dist / Mathf.Max(1f, pixelsPerSecond));

        SoundManager.Instance.PlaySFX("projectile_fire");
        int loopHandle = SoundManager.Instance.PlayLoopSFX("projectile_loop", proj.transform, true, volume: 0.7f);
        // Simple quadratic bezier for a nice arc
        Vector2 mid = (start + end) * 0.5f + Vector2.up * arcPixels;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float u = 1f - Mathf.Clamp01(t);
            // Quadratic Bezier: B(t) = u^2*P0 + 2*u*t*Pm + t^2*P1
            Vector2 pos = (u * u) * start + (2f * u * t) * mid + (t * t) * end;
            proj.anchoredPosition = pos;
            yield return null;
        }

        proj.anchoredPosition = end;

        yield return StartCoroutine(SoundManager.Instance.FadeOutAndStopLoop(loopHandle, 0.06f));
        //SoundManager.Instance.PlaySFX("projectile_impact");

        // Optional: tiny impact pulse on hit card (if you have such a method)
        // var tgtCardUI = to.GetComponent<CharacterCardUI>();
        // tgtCardUI?.ShakeCard();

        Destroy(go);
    }
    
    public IEnumerator PlayProjectiles(GameCharacter source, List<GameCharacter> targets, DamageType type)
    {
        var srcCard = activeCharPanel.FindCardForCharacter(source);
        if (srcCard == null || targets == null || targets.Count == 0) yield break;

        var from = srcCard.GetComponent<RectTransform>();
        Sprite sprite = GetSpriteFor(type);

        int remaining = 0;

        // Fire one projectile per target, in parallel, then wait until all finish
        foreach (var t in targets)
        {
            var tgtCard = activeCharPanel.FindCardForCharacter(t);
            if (tgtCard == null) continue;

            remaining++;
            StartCoroutine(FireAndSignal(from, tgtCard.GetComponent<RectTransform>(), sprite, () => remaining--));
        }

        while (remaining > 0) yield return null;
    }

    public IEnumerator PlayVolley(GameCharacter source, List<HitResolution> resolutions, DamageType type)
    {
        if (resolutions == null || resolutions.Count == 0) yield break;

        var srcCard = activeCharPanel.FindCardForCharacter(source);
        if (srcCard == null) yield break;

        RectTransform from = srcCard.GetComponent<RectTransform>();
        Sprite sprite = GetSpriteFor(type);

        int remaining = 0;

        foreach (var res in resolutions)
        {
            if (res.Target == null) continue;

            var tgtCard = activeCharPanel.FindCardForCharacter(res.Target);
            if (tgtCard == null) continue;

            RectTransform to = tgtCard.GetComponent<RectTransform>();
            //Debug.Log($"Volley plan → {res.Target.Name} WillHit={res.WillHit}");
            // Fire projectile (parallel)
            remaining++;
            StartCoroutine(FireAndSignal(from, to, sprite, () => remaining--));

            // Schedule dodge if this one is a miss
            if (!res.WillHit)
            {
                remaining++;
                // Match FireProjectile’s duration calc
                Vector2 start = from.anchoredPosition;
                Vector2 end   = to.anchoredPosition;
                float dist     = Vector2.Distance(start, end);
                float duration = Mathf.Max(0.05f, dist / Mathf.Max(1f, pixelsPerSecond));

                StartCoroutine(DodgeAndSignal(tgtCard, from.anchoredPosition, to.anchoredPosition, duration, () => remaining--));
            }
        }

        while (remaining > 0) yield return null;
    }
    private IEnumerator FireAndSignal(RectTransform from, RectTransform to, Sprite sprite, System.Action onDone)
    {
        yield return StartCoroutine(FireProjectile(from, to, sprite));
        onDone?.Invoke();
    }
    private IEnumerator DodgeAndSignal(CharacterCardUI targetCard, Vector2 projStart, Vector2 projEnd, float flightDuration, System.Action onDone)
    {
        yield return StartCoroutine(DodgeCardNearImpact(targetCard, projStart, projEnd, flightDuration));
        onDone?.Invoke();
    }


    private IEnumerator DodgeCardNearImpact(CharacterCardUI targetCard, Vector2 projStart, Vector2 projEnd, float flightDuration)
    {
        if (targetCard == null) yield break;
        var rect = targetCard.GetComponent<RectTransform>();
        Vector2 original = rect.anchoredPosition;

        // Wait until near "impact"
        float wait = Mathf.Clamp01(1f - dodgeImpactLead) * flightDuration;
        if (wait > 0f) yield return new WaitForSeconds(wait);

        // Perpendicular to projectile path; dodge outward from center (simple rule)
        Vector2 dir = (projEnd - projStart).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x);
        float sign = Mathf.Sign(original.x == 0 ? 1f : original.x); // push away from center
        Vector2 dodgePos = original + perp * (dodgeOffsetPixels * sign);

        // Step out, then back (reuse your existing MoveToPosition)
        targetCard.CancelActiveMove();
        yield return StartCoroutine(targetCard.MoveToPosition(dodgePos, dodgeOutDuration));
        SoundManager.Instance.PlaySFX("miss");

        targetCard.CancelActiveMove();
        yield return StartCoroutine(targetCard.MoveToPosition(original, dodgeReturnDuration));

        // OPTIONAL: if HP bar to follow during dodge and have card.HPBar:
        // if (targetCard.HPBar != null) {
        //     yield return StartCoroutine(targetCard.HPBar.MoveToPosition(dodgePos + new Vector2(0, 92f), dodgeOutDuration));
        //     yield return StartCoroutine(targetCard.HPBar.MoveToPosition(original + new Vector2(0, 92f), dodgeReturnDuration));
        // }
    }


}
