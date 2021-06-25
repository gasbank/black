using UnityEngine;
using UnityEngine.EventSystems;

public class Pan : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [SerializeField]
    Transform targetImage;

    [SerializeField]
    Vector3 beginDragWorldPosition;

    [SerializeField]
    Vector3 beginDragTargetPosition;

    [SerializeField]
    bool panning;

    [SerializeField]
    RectTransform rt;

    [SerializeField]
    MainGame mainGame;

    static bool panningMutex;

#if UNITY_EDITOR
    void OnValidate() {
        rt = GetComponent<RectTransform>();
    }
#endif

    public void OnBeginDrag(PointerEventData eventData) {
        if (PinchZoom.PinchZooming == false && panningMutex == false) {
            panningMutex = true;
            panning = true;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, eventData.position, Camera.main,
                out beginDragWorldPosition);
            beginDragTargetPosition = targetImage.position;
        }
    }

    public void OnDrag(PointerEventData eventData) {
        if (panning && mainGame.CanInteractPanAndZoom) {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, eventData.position, Camera.main,
                out var dragWorldPosition);
            targetImage.position = beginDragTargetPosition + (dragWorldPosition - beginDragWorldPosition);
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (panningMutex) {
            panningMutex = false;
            panning = false;
        }
    }
}