using UnityEngine;

[DisallowMultipleComponent]
public class BottomNotchOffsetGroup : MonoBehaviour {
    [SerializeField]
    RectTransform rt;
    [SerializeField]
    float notNotchMargin = 0;
    [SerializeField]
    float notchMargin = 30;
    [SerializeField]
    bool isBottomPivot = false;

#if UNITY_EDITOR
    void OnValidate() {
        rt = GetComponent<RectTransform>();
    }
#endif

    public bool NotchMarginActive {
        get {
            if (isBottomPivot) {
                return rt.anchoredPosition.y < notNotchMargin;
            } else {
                return rt.offsetMin.y > notNotchMargin;
            }
        }
        set {
            if (isBottomPivot) {
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, value ? notchMargin : notNotchMargin);
            } else {
                rt.offsetMin = new Vector2(rt.offsetMin.x, value ? notchMargin : notNotchMargin);
            }
        }
    }
}
