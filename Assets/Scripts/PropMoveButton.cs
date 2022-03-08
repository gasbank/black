using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class PropMoveButton : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    Vector2 dragDelta;
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif
    public void OnBeginDrag(PointerEventData eventData)
    {
        dragDelta = ActivePropButtonGroup.Instance.ActiveProp.ScreenPosition - eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        InteriorRaycaster.Instance.UpdateActivePropPosition(eventData.position + dragDelta);
    }
}