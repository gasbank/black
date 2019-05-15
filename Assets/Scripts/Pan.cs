using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Pan : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [SerializeField] Transform targetImage = null;
    [SerializeField] Vector3 beginDragWorldPosition;
    [SerializeField] Vector3 beginDragTargetPosition;
    [SerializeField] bool panning = false;
    static bool panningMutex = false;

    public void OnBeginDrag(PointerEventData eventData) {
        if (PinchZoom.PinchZooming == false && panningMutex == false) {
            panningMutex = true;
            panning = true;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(GetComponent<RectTransform>(), eventData.position, Camera.main, out beginDragWorldPosition);
            beginDragTargetPosition = targetImage.position;
        }
    }

    public void OnDrag(PointerEventData eventData) {
        if (panning) {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(GetComponent<RectTransform>(), eventData.position, Camera.main, out Vector3 dragWorldPosition);
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
