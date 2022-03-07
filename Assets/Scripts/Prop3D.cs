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

    public BoxCollider BoxCollider => boxCollider;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif
    public void MoveByScreenPoint(Vector3 forward, Vector3 worldPoint, Vector2 screenPoint)
    {
        if (transform == null)
        {
            return;
        }
        
        transform.position = worldPoint;
        transform.LookAt(forward);
        prop.MoveByScreenPoint(screenPoint);
        ActivePropButtonGroup.Instance.HideButtonGroup();
    }
}