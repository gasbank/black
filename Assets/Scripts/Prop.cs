using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class Prop : MonoBehaviour
{   
    [SerializeField]
    Camera cam;

    [SerializeField]
    RectTransform rt;

    [FormerlySerializedAs("placedProp3D")]
    [SerializeField]
    Prop3D prop3D;

    public Prop3D Prop3D => prop3D;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif
    
    public void MoveByScreenPoint(Vector2 screenPoint)
    {
        var parentRt = transform.parent.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, screenPoint, cam, out var localPoint))
        {
            rt.anchoredPosition = localPoint;
        }
    }

    public void OnClick()
    {
        ActivePropButtonGroup.Instance.ActiveProp = this;
        ActivePropButtonGroup.Instance.ToggleButtonGroup();
    }
}