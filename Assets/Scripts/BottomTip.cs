using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BottomTip : MonoBehaviour
{
    [SerializeField]
    Text text;

    [SerializeField]
    Subcanvas subcanvas;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void OpenPopup()
    {
    }

    void ClosePopup()
    {
    }

    public void SetMessage(string message)
    {
        text.text = message;
    }

    public void CloseSubcanvas()
    {
        subcanvas.Close();
    }

    public void OpenSubcanvas()
    {
        subcanvas.Open();
    }
}