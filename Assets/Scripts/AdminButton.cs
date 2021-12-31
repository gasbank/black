using UnityEngine;

[DisallowMultipleComponent]
public class AdminButton : MonoBehaviour
{
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Awake()
    {
#if !BLACK_ADMIN
        gameObject.SetActive(false);
#endif
    }
}