using UnityEngine;
using TMPro;
using System.Collections;

public class BannerScroller : MonoBehaviour
{
    [SerializeField] private RectTransform bannerTransform;
    [SerializeField] private RectTransform textTransform;
    [SerializeField] private float scrollSpeed = 50f;

    private float resetX;
    private float startX;

    void Start()
    {
        StartCoroutine(InitBanner());
    }

    private IEnumerator InitBanner()
    {
        yield return new WaitForEndOfFrame(); // Wait for layout

        float bannerWidth = bannerTransform.rect.width;
        float textWidth = textTransform.rect.width;

        startX = bannerWidth;             // Start just off the right edge
        resetX = -textWidth - bannerTransform.rect.width;;              // Reset once it's off the left edge

        textTransform.anchoredPosition = new Vector2(startX, textTransform.anchoredPosition.y);
    }

    void Update()
    {
        if (textTransform == null) return;

        textTransform.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;

        if (textTransform.anchoredPosition.x < resetX)
        {
            textTransform.anchoredPosition = new Vector2(startX, textTransform.anchoredPosition.y);
        }
    }
}

