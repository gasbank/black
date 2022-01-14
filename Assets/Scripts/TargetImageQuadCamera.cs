using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class TargetImageQuadCamera : MonoBehaviour
{
    [SerializeField]
    Camera cam;

    [SerializeField]
    IslandShader3DController islandShader3DController;

    bool clearOnce;
    
    readonly Queue<int> islandIndexQueue = new Queue<int>();

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
        if (islandIndexQueue.Count > 0)
        {
            var islandIndex = islandIndexQueue.Dequeue();
            islandShader3DController.SetIslandIndex(islandIndex);
        }
        
        if (clearOnce) return;
        
        cam.clearFlags = CameraClearFlags.SolidColor;
    }

    void OnPostRender()
    {
        cam.enabled = islandIndexQueue.Count > 0;
        
        if (clearOnce) return;
        
        cam.clearFlags = CameraClearFlags.Nothing;
        
        clearOnce = true;
    }

    public void EnqueueIslandIndex(int islandIndex)
    {
        islandIndexQueue.Enqueue(islandIndex);
    }

    public void ClearQueue()
    {
        islandIndexQueue.Clear();
    }
}