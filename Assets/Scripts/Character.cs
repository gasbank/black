using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Character : MonoBehaviour
{
    [SerializeField]
    Character3D char3D;

    [SerializeField]
    RectTransform rt;

    [SerializeField]
    RectTransform parentRt;

    [SerializeField]
    Camera cam;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);

        rt = GetComponent<RectTransform>();
        parentRt = transform.parent.GetComponent<RectTransform>();
    }
#endif

    void Update()
    {
        FollowChar3D();
    }

    void FollowChar3D()
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, char3D.ScreenPoint, cam, out var localPoint))
        {
            rt.anchoredPosition = localPoint;
        }

        var localScale = transform.localScale;
        localScale.x = char3D.VelocityOnScreenPoint.x > 0 ? -1 : 1;
        transform.localScale = localScale;
    }
}