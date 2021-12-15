using UnityEngine;

[DisallowMultipleComponent]
public class BottomNotchOffsetGroup : MonoBehaviour
{
    [SerializeField]
    bool isBottomPivot;

    [SerializeField]
    float notchMargin = 30;

    [SerializeField]
    float notNotchMargin;

    [SerializeField]
    RectTransform rt;

    public bool NotchMarginActive
    {
        get
        {
            if (isBottomPivot)
                return rt.anchoredPosition.y < notNotchMargin;
            return rt.offsetMin.y > notNotchMargin;
        }
        set
        {
            if (isBottomPivot)
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, value ? notchMargin : notNotchMargin);
            else
                rt.offsetMin = new Vector2(rt.offsetMin.x, value ? notchMargin : notNotchMargin);
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        rt = GetComponent<RectTransform>();
    }
#endif
}