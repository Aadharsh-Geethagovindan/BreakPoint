using UnityEngine;
using UnityEngine.EventSystems;

public class HistoryBoxHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Logger.Instance?.SetAllTransparency(1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Logger.Instance?.SetDynamicTransparency();
    }
}
