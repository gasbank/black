using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PaletteGroup : MonoBehaviour
{
    [SerializeField]
    ScrollRect scrollRect;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    public void EnsureReveal()
    {
        //scrollRect.
    }
}