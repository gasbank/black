using UnityEngine;
using UnityEngine.EventSystems;

public class Pan : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    static bool panningMutex;

    [SerializeField]
    Vector3 beginDragTargetPosition;

    [SerializeField]
    Vector3 beginDragWorldPosition;

    [SerializeField]
    MainGame mainGame;

    [SerializeField]
    bool panning;

    [SerializeField]
    RectTransform rt;

    [SerializeField]
    RectTransform targetImage;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (PinchZoom.PinchZooming == false && panningMutex == false)
        {
            panningMutex = true;
            panning = true;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, eventData.position, Camera.main,
                out beginDragWorldPosition);
            beginDragTargetPosition = targetImage.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (panning && mainGame.CanInteractPanAndZoom)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, eventData.position, Camera.main,
                out var dragWorldPosition);
            targetImage.position = beginDragTargetPosition + (dragWorldPosition - beginDragWorldPosition);
            LimitDragRange();
        }
    }

    void LimitDragRange()
    {
        var anchoredPosition = targetImage.anchoredPosition;
        anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, -500, 500);
        anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, -500, 500);
        targetImage.anchoredPosition = anchoredPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (panningMutex)
        {
            panningMutex = false;
            panning = false;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        rt = GetComponent<RectTransform>();
    }
#endif
}