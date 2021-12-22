using UnityEngine;

[DisallowMultipleComponent]
public class GoldGroup : MonoBehaviour {
#if UNITY_EDITOR
    void OnValidate() {
        AutoBindUtil.BindAll(this);
    }
#endif
}
