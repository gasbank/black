using UnityEngine;
using UnityEngine.EventSystems;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class Pan : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
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
        if (PinchZoom.PinchZooming == false)
        {
            panning = true;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, eventData.position, Camera.main,
                out beginDragWorldPosition);
            beginDragTargetPosition = targetImage.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Touch.activeTouches.Count != 1)
        {
            return;
        }
        
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
        panning = false;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        rt = GetComponent<RectTransform>();
    }
#endif
}