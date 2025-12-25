using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class CharacterCardUI : MonoBehaviour
{
    public Image outlineImage;
    public Image targetedImage;
    public Button cardButton;
    public TMP_Text nameText;
    private GameCharacter characterRef;
    private Action<GameCharacter> onClickAction;
    public Vector2 ClockPosition { get; set; }
    public Slider shieldSlider; // Reference to the shield slider
    public RectTransform HPBar { get; set; }
   

    public StatusEffectDisplay statusEffectDisplay;

    public bool setRecent = true;
    private Coroutine _moveRoutine;
    public TMP_Text speedRollText; 
    private GameCharacter boundCharacter;

    private Coroutine _activeTurnRoutine;
    private bool _isActiveTurn;

    [SerializeField] private RectTransform breatheTarget;

    private TMPro.VertexGradient _baseGrad;
    private TMPro.VertexGradient _pulseGrad;
    private bool _hasBaseGrad;

// Breathing
    private Vector3 _baseScale;
    private Vector3 _pulseScale;


    void OnEnable()
    {
        EventManager.Subscribe("OnSpeedRoll", HandleSpeedRoll);
    }

    void OnDisable()
    {
        EventManager.Unsubscribe("OnSpeedRoll", HandleSpeedRoll);
    }
    private void Start()
    {
        if (breatheTarget == null)
            breatheTarget = (RectTransform)transform;

        _baseScale = breatheTarget.localScale;
        _pulseScale = _baseScale * 1.025f; // subtle

        if (nameText != null)
        {
            // Ensure gradient is enabled
            nameText.enableVertexGradient = true;
            nameText.colorGradient = new TMPro.VertexGradient(nameText.color);


            _baseGrad = nameText.colorGradient;
            _hasBaseGrad = true;

            // Build a pulse gradient based on your current cyan:
            // brighten slightly toward white (not neon)
            Color topL = Color.Lerp(_baseGrad.topLeft, Color.green, 0.55f);
            Color topR = Color.Lerp(_baseGrad.topRight, Color.green, 0.55f);
            Color botL = Color.Lerp(_baseGrad.bottomLeft, Color.green, 0.10f);
            Color botR = Color.Lerp(_baseGrad.bottomRight, Color.green, 0.10f);

            _pulseGrad = new TMPro.VertexGradient(topL, topR, botL, botR);
        }
    }

    public Coroutine StartMoveExclusive(Vector2 targetPos, float duration)
    {
        if (_moveRoutine != null)
            StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveToPosition(targetPos, duration));
        return _moveRoutine;
    }

    public void CancelActiveMove()
    {
        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = null;
    }

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

    public void RefreshStatusEffectsFromSnapshot(List<StatusEffectState> statusStates)
    {
        if (setRecent)
        {
            statusEffectDisplay.recentStatusImage = GameObject.Find("RecentStatusImage")?.GetComponent<Image>();
            setRecent = false;
        }

        if (statusEffectDisplay != null)
        {
            statusEffectDisplay.UpdateStatusEffectDisplayFromSnapshot(statusStates);
        }
    }
    private IEnumerator ActiveTurnLoop()
    {
        float t = 0f;
        float t2 = 0f;
        while (true)
        {
            //float pulse = (Mathf.Sin(t * Mathf.PI * 2f) + 1f) * 0.5f; // 0..1
            float pulse = Mathf.PingPong(t2, 1f);
            // Name gradient
            if (nameText != null && _hasBaseGrad)
            {
                nameText.colorGradient = LerpGradient(_baseGrad, _pulseGrad, pulse);
                nameText.SetVerticesDirty(); // force update
            }

            // Breathing scale
            if (breatheTarget != null)
                breatheTarget.localScale = Vector3.Lerp(_baseScale, _pulseScale, pulse);

            t += Time.deltaTime * 0.25f; // slow
            t2 += Time.deltaTime * 0.85f;
            yield return null;
        }
    }

    private TMPro.VertexGradient LerpGradient(TMPro.VertexGradient a, TMPro.VertexGradient b, float t)
    {
        return new TMPro.VertexGradient(
            Color.Lerp(a.topLeft, b.topLeft, t),
            Color.Lerp(a.topRight, b.topRight, t),
            Color.Lerp(a.bottomLeft, b.bottomLeft, t),
            Color.Lerp(a.bottomRight, b.bottomRight, t)
        );
    }


    public IEnumerator MoveToPosition(Vector2 targetPos, float duration)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 start = rectTransform.anchoredPosition;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            rectTransform.anchoredPosition = Vector2.Lerp(start, targetPos, t);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
    }
    
    public void BindCharacter(GameCharacter character)
    {
        boundCharacter = character;
        if (speedRollText != null) speedRollText.text = ""; // clear at bind
    }

    public void ApplySnapshotHP(int hp, int maxHp)
    {
        if (HPBar == null) return;

        var barController = HPBar.GetComponent<HPBarController>();
        if (barController == null) return;

        barController.ApplySnapshotHP(hp, maxHp);
    }

    private void HandleSpeedRoll(object data)
    {
        var evt = data as GameEventData;
        if (evt == null) return;

        GameCharacter c = evt.Get<GameCharacter>("Character");
        if (c != boundCharacter) return; // not my card

        int roll = evt.Get<int>("Roll");
        float mod = evt.Get<float>("Mod");
        float total = evt.Get<float>("Total");

        // Show the result as "15 + 6.1 = 21.1"
        if (speedRollText != null)
        {
            speedRollText.text = $"{roll} + {mod:0.0} = {total:0.0}";
            speedRollText.gameObject.SetActive(true);

            // Optional: fade out after 1s
            CancelInvoke(nameof(HideSpeedRoll));
            Invoke(nameof(HideSpeedRoll), 5.5f);
        }
    }

    private void HideSpeedRoll()
    {
        if (speedRollText != null)
            speedRollText.gameObject.SetActive(false);
    }

    public void SetActiveTurnVisual(bool active)
    {
        if (_isActiveTurn == active) return;
        _isActiveTurn = active;

        if (_activeTurnRoutine != null)
            StopCoroutine(_activeTurnRoutine);

        if (active)
            _activeTurnRoutine = StartCoroutine(ActiveTurnLoop());
        else
            ResetActiveTurnVisuals();
    }

    private void ResetActiveTurnVisuals()
    {
        if (nameText != null && _hasBaseGrad)
        {
            nameText.colorGradient = _baseGrad;
            nameText.SetVerticesDirty();
        }

        if (breatheTarget != null)
            breatheTarget.localScale = _baseScale;
    }

}

public static class UIAnimationUtils
{
    public static IEnumerator MoveToPosition(this RectTransform rectTransform, Vector2 target, float duration)
    {
        Vector2 start = rectTransform.anchoredPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            rectTransform.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        rectTransform.anchoredPosition = target;
    }
}