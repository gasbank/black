using JetBrains.Annotations;
using UnityEngine;

[DisallowMultipleComponent]
public class ProfilePopup : MonoBehaviour
{
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    [UsedImplicitly]
    void OpenPopup()
    {
        
    }

    [UsedImplicitly]
    void ClosePopup()
    {
        
    }
}