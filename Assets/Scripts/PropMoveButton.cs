using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class PropMoveButton : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Vector2 dragDelta;
    Vector3 lastNonoverlappedPosition3D;
    Vector3 lastNonoverlappedForwardPoint;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif
    public void OnBeginDrag(PointerEventData eventData)
    {
        UpdateLastNonoverlappedTransform();

        dragDelta = ActivePropButtonGroup.Instance.ActiveProp.ScreenPosition - eventData.position;
    }

    void UpdateLastNonoverlappedTransform()
    {
        var activeProp3DTransform = ActivePropButtonGroup.Instance.ActiveProp3D.transform;
        lastNonoverlappedPosition3D = activeProp3DTransform.position;
        lastNonoverlappedForwardPoint = lastNonoverlappedPosition3D + activeProp3DTransform.forward;
    }

    public void OnDrag(PointerEventData eventData)
    {
        InteriorRaycaster.Instance.UpdateActivePropPosition(eventData.position + dragDelta);

        if (ActivePropButtonGroup.Instance.ActiveProp3D.Overlapped == false)
        {
            UpdateLastNonoverlappedTransform();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ActivePropButtonGroup.Instance.ActiveProp3D.Overlapped)
        {
            InteriorRaycaster.Instance.UpdateActivePropPositionBy3D(ActivePropButtonGroup.Instance.ActiveProp,
                lastNonoverlappedForwardPoint, lastNonoverlappedPosition3D);
        }
    }
}