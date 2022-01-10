using System;
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

    void Start()
    {
        RenderOneFrame();
    }

    public void RenderOneFrame()
    {
        cam.enabled = true;
    }

    public void ClearCameraOnce()
    {
        clearOnce = false;
        RenderOneFrame();
    }

    void OnPreRender()
    {
        if (clearOnce) return;
        
        cam.clearFlags = CameraClearFlags.SolidColor;
    }

    void OnPostRender()
    {
        cam.enabled = false;
        
        if (clearOnce) return;
        
        cam.clearFlags = CameraClearFlags.Nothing;
        
        clearOnce = true;
    }
}