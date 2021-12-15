using UnityEngine;

[DisallowMultipleComponent]
public class TopNotchOffsetGroup : MonoBehaviour {
    [SerializeField]
    RectTransform rt;
    [SerializeField]
    float notNotchMargin = 0;
    [SerializeField]
    float notchMargin = -40;

#if UNITY_EDITOR
    void OnValidate() {
        rt = GetComponent<RectTransform>();
    }
#endif

    public bool NotchMarginActive {
        get => rt.offsetMax.y < notNotchMargin;
        set => rt.offsetMax = new Vector2(rt.offsetMax.x, value ? notchMargin : notNotchMargin);
    }
}
