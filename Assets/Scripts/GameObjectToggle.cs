using UnityEngine;

[DisallowMultipleComponent]
public class GameObjectToggle : MonoBehaviour
{
    [SerializeField]
    CanvasGroup canvasGroup;

    [SerializeField]
    bool suppressMessage;

    public bool Active => canvasGroup.alpha != 0;

#if UNITY_EDITOR
    void OnValidate()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
#endif

    public void Toggle()
    {
        canvasGroup.SetActiveCheap(Active == false);
        if (suppressMessage == false)
        {
            if (Active)
                SendMessage("OnOpenPopup");
            else
                SendMessage("OnClosePopup");
        }
    }

    public void Open()
    {
        if (Active == false) Toggle();
    }

    public void Close()
    {
        if (Active) Toggle();
    }
}