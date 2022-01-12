using JetBrains.Annotations;
using UnityEngine;

[DisallowMultipleComponent]
public class AdminButtonGroup : MonoBehaviour
{
    [SerializeField]
    Subcanvas subcanvas;
    
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

    public void Close()
    {
        subcanvas.Close();
    }
}