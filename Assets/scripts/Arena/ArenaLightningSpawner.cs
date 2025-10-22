using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ArenaLightningSpawner : MonoBehaviour
{
    [Header("References")]
    public RectTransform arenaArea;   // large rect covering background
    public GameObject boltPrefab;     // your UILightningBolt prefab

    [Header("Timing")]
    public float minDelay = 2f;
    public float maxDelay = 6f;
    public float boltLifetime = 0.25f;

    [Header("Visuals")]
    public float minScale = 0.8f;
    public float maxScale = 1.5f;
    public float maxRotation = 45f;

    [Header("Energy Flash")]
    public Image[] backgroundLines;     // optional: neon lines or glow strips to flash
    public float flashDuration = 0.15f;
    public float flashIntensity = 1.6f;

    private void Start() => StartCoroutine(SpawnLoop());

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
            SpawnBolt();
        }
    }

    private void SpawnBolt()
    {
        if (boltPrefab == null || arenaArea == null) return;

        GameObject bolt = Instantiate(boltPrefab, arenaArea);
        RectTransform r = bolt.GetComponent<RectTransform>();

        // random position within arena rect
        Vector2 randPos = new Vector2(
            Random.Range(0, arenaArea.rect.width),
            Random.Range(0, arenaArea.rect.height)
        );
        r.anchoredPosition = randPos - arenaArea.rect.size / 2f;

        // random rotation + scale
        r.localRotation = Quaternion.Euler(0, 0, Random.Range(-maxRotation, maxRotation));
        r.localScale = Vector3.one * Random.Range(minScale, maxScale);

        // quick fade coroutine
        Image img = bolt.GetComponentInChildren<Image>();
        StartCoroutine(FadeAndDestroy(img, boltLifetime));

        // Trigger quick energy flash on background lines
        if (backgroundLines != null && backgroundLines.Length > 0)
            StartCoroutine(FlashBackgroundLines());
    }

    private IEnumerator FadeAndDestroy(Image img, float duration)
    {
        float t = 0f;
        Color c = img.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / duration);
            img.color = c;
            yield return null;
        }
        Destroy(img.gameObject.transform.parent.gameObject);
    }

    private IEnumerator FlashBackgroundLines()
    {
        foreach (var line in backgroundLines)
        {
            if (line == null) continue;
            Color baseColor = line.color;
            Color flashColor = baseColor * flashIntensity;
            flashColor.a = baseColor.a;  // preserve alpha
            line.color = flashColor;
        }

        yield return new WaitForSeconds(flashDuration);

        foreach (var line in backgroundLines)
        {
            if (line == null) continue;
            // smooth return to original color
            line.CrossFadeColor(line.color / flashIntensity, 0.3f, false, true);
        }
    }
}
