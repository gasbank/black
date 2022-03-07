using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class InterfaceCanvasRaycaster : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
#if UNITY_EDITOR
    void OnValidate() {
        AutoBindUtil.BindAll(this);
    }
#endif
    public void OnPointerDown(PointerEventData eventData)
    {
        InteriorRaycaster.Instance.UpdateActiveDebrisPosition(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        
    }
}
