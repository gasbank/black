using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Subcanvas))]
public class HideOnLiveBuild : MonoBehaviour
{
    [SerializeField]
    [AutoBindThis]
    Subcanvas subcanvas;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    public void Hide()
    {
        subcanvas.Close();
    }
}