using JetBrains.Annotations;
using UnityEngine;

[DisallowMultipleComponent]
public class MainDebugGroup : MonoBehaviour
{
    [SerializeField]
    Subcanvas subcanvas;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Start()
    {
        if (Application.isEditor)
        {
            subcanvas.Open();
        }
    }

    [UsedImplicitly]
    void OpenPopup()
    {
    }

    [UsedImplicitly]
    void ClosePopup()
    {
    }
}