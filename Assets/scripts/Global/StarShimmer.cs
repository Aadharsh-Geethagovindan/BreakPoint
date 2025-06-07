using UnityEngine;
using UnityEngine.UI;  // Only if using UI Image

public class StarShimmer : MonoBehaviour
{
    [Header("Settings")]
    public float minWaitTime = 1f;     // Minimum seconds before shimmer
    public float maxWaitTime = 5f;     // Maximum seconds before shimmer (make this editable per star)

    public float shimmerDuration = 0.5f; // How long it takes to shimmer in/out
    public float shimmerAmount = 0.5f;   // How much brighter it gets (0 to 1 extra alpha)

    private float baseAlpha;
    private Image image;               // If using UI
    private SpriteRenderer spriteRenderer; // If using SpriteRenderer

    void Start()
    {
        image = GetComponent<Image>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (image != null)
            baseAlpha = image.color.a;
        else if (spriteRenderer != null)
            baseAlpha = spriteRenderer.color.a;

        StartCoroutine(ShimmerLoop());
    }

    private System.Collections.IEnumerator ShimmerLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);

            // Brighten
            yield return StartCoroutine(ChangeAlpha(baseAlpha + shimmerAmount));

            // Dim back
            yield return StartCoroutine(ChangeAlpha(baseAlpha));
        }
    }

    private System.Collections.IEnumerator ChangeAlpha(float targetAlpha)
    {
        float elapsed = 0f;
        float startAlpha = (image != null) ? image.color.a : spriteRenderer.color.a;

        while (elapsed < shimmerDuration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / shimmerDuration);

            if (image != null)
            {
                Color c = image.color;
                c.a = newAlpha;
                image.color = c;
            }
            else if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = newAlpha;
                spriteRenderer.color = c;
            }

            yield return null;
        }
    }
}
