using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class HoverOverlayController : MonoBehaviour
{
    [SerializeField] GameObject overlayPanel;
     [SerializeField] Image baseTagImage;
    [SerializeField] float hideDelay = 0.12f;

    int hoverRefs = 0;
    Coroutine hideCo;

    void Awake()
    {
        if (overlayPanel) overlayPanel.SetActive(false);
    }

    public void OnRegionEnter()
    {
        hoverRefs++;
        if (hideCo != null) { StopCoroutine(hideCo); hideCo = null; }
        if (!overlayPanel.activeSelf) overlayPanel.SetActive(true);
        if (baseTagImage && baseTagImage.enabled) baseTagImage.enabled = false;
    }

    public void OnRegionExit()
    {
        hoverRefs = Mathf.Max(0, hoverRefs - 1);
        if (hoverRefs == 0 && hideCo == null)
            hideCo = StartCoroutine(HideAfterDelay());
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        if (hoverRefs == 0)
        {
            if (overlayPanel.activeSelf) overlayPanel.SetActive(false);
            if (baseTagImage && !baseTagImage.enabled) baseTagImage.enabled = true;
        }
        hideCo = null;
    }
}
