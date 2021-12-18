using JetBrains.Annotations;
using UnityEngine;

[DisallowMultipleComponent]
public class NoticePopup : MonoBehaviour
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

    [UsedImplicitly]
    public void OnButton1Click()
    {
        subcanvas.Close();
    }
}