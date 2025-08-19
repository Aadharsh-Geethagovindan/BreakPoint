using UnityEngine;
using UnityEngine.EventSystems;

public class HoverRegion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public HoverOverlayController controller;

    public void OnPointerEnter(PointerEventData eventData) => controller.OnRegionEnter();
    public void OnPointerExit(PointerEventData eventData)  => controller.OnRegionExit();
}
