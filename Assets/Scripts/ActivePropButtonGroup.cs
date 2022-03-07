using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class ActivePropButtonGroup : MonoBehaviour
{
    public static ActivePropButtonGroup Instance;
    
    [FormerlySerializedAs("activeMuseumDebris")]
    [SerializeField]
    Prop activeProp;

    [SerializeField]
    CanvasGroupAlpha canvasGroupAlpha;

    public Prop3D ActiveProp3D => activeProp != null ? activeProp.Prop3D : null;

    public Prop ActiveProp
    {
        get => activeProp;
        set => activeProp = value;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (activeProp == null)
        {
            return;
        }

        transform.position = activeProp.transform.position;
    }

    public void ToggleButtonGroup()
    {
        canvasGroupAlpha.SetAlphaImmediately(canvasGroupAlpha.TargetAlpha > 0 ? 0 : 1);
    }

    public void HideButtonGroup()
    {
        canvasGroupAlpha.SetAlphaImmediately(0);
    }
}