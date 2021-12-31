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
#if !BLACK_ADMIN
        gameObject.SetActive(false);
#endif
    }
}
