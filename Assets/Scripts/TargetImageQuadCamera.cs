using UnityEngine;

[DisallowMultipleComponent]
public class TargetImageQuadCamera : MonoBehaviour
{
    [SerializeField]
    Camera cam;

    bool clearOnce;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    public void ClearCameraOnce() => clearOnce = false;

    void OnPreRender()
    {
        if (clearOnce) return;
        
        cam.clearFlags = CameraClearFlags.SolidColor;
    }

    void OnPostRender()
    {
        if (clearOnce) return;
        
        cam.clearFlags = CameraClearFlags.Nothing;
        
        clearOnce = true;
    }
}