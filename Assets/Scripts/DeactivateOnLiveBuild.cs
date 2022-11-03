using UnityEngine;

[DisallowMultipleComponent]
public class DeactivateOnLiveBuild : MonoBehaviour
{
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Awake()
    {
#if !DEV_BUILD
        gameObject.SetActive(false);
#endif
    }
}
