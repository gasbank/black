using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class Prop3D : MonoBehaviour
{
    [FormerlySerializedAs("placedProp")]
    [FormerlySerializedAs("museumDebris")]
    [SerializeField]
    Prop prop;

    [SerializeField]
    BoxCollider boxCollider;

    [FormerlySerializedAs("attachLayer")]
    [SerializeField]
    LayerMask attachRaycastLayer;

    [SerializeField]
    Camera interiorCam;

    public BoxCollider BoxCollider => boxCollider;

    public LayerMask AttachRaycastLayer => attachRaycastLayer;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    [ContextMenu("Apply To Prop")]
    void ApplyToProp()
    {
        var screenPoint = RectTransformUtility.WorldToScreenPoint(interiorCam, transform.position);
        MoveByScreenPoint(transform.position + transform.forward, transform.position, screenPoint);
    }
    
    public void MoveByScreenPoint(Vector3 forward, Vector3 worldPoint, Vector2 screenPoint)
    {
        if (transform == null)
        {
            return;
        }
        
        transform.position = worldPoint;
        transform.LookAt(forward);
        prop.MoveByScreenPoint(screenPoint);
        if (ActivePropButtonGroup.Instance != null)
        {
            ActivePropButtonGroup.Instance.HideButtonGroup();
        }
    }
}