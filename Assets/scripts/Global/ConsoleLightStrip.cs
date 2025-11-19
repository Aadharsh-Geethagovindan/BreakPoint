using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleLightStrip : MonoBehaviour
{
    [Header("Config")]
    public float minInterval = 0.3f;
    public float maxInterval = 1.2f;
    public bool sequentialMode = false;    // if true, lights sweep left→right→left
    public bool randomizeColors = false;
    public Color activeColor = Color.cyan;
    public Color inactiveColor = new Color(0, 0, 0, 0.25f);

    private Image[] lights;
    private int index = 0;
    private int direction = 1;

    private void Awake()
    {
        int childCount = transform.childCount;
        lights = new Image[childCount];

        for (int i = 0; i < childCount; i++)
            lights[i] = transform.GetChild(i).GetComponent<Image>();

        foreach (var l in lights)
            l.color = inactiveColor;
    }

    private void OnEnable() => StartCoroutine(AnimateLights());
    private void OnDisable() => StopAllCoroutines();

    private IEnumerator AnimateLights()
    {
        while (true)
        {
            if (lights.Length == 0) yield break;

            // Reset all
            foreach (var l in lights) l.color = inactiveColor;

            if (sequentialMode)
            {
                lights[index].color = randomizeColors ? Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.9f, 1f) : activeColor;

                index += direction;
                if (index >= lights.Length || index < 0)
                {
                    direction *= -1;
                    index += direction;
                }
            }
            else // random flicker mode
            {
                int r = Random.Range(0, lights.Length);
                lights[r].color = randomizeColors ? Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.9f, 1f) : activeColor;
            }

            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
        }
    }
}
