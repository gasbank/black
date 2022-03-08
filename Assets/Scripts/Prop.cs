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

    [SerializeField]
    bool flipOnNegativeForward;

    public Prop3D Prop3D => prop3D;
    public Vector2 ScreenPosition => RectTransformUtility.WorldToScreenPoint(cam, transform.position);

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif
    
    public void MoveByScreenPoint(Vector2 screenPoint, bool facingLeft)
    {
        var parentRt = transform.parent.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, screenPoint, cam, out var localPoint))
        {
            rt.anchoredPosition = localPoint;
        }

        if (flipOnNegativeForward)
        {
            var localScale = rt.localScale;
            localScale.x = facingLeft ? -1 : 1;
            rt.localScale = localScale;
        }
    }

    public void OnClick()
    {
        ActivePropButtonGroup.Instance.ActiveProp = this;
        ActivePropButtonGroup.Instance.ToggleButtonGroup();
    }
}