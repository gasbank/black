using UnityEngine;

[DisallowMultipleComponent]
[ExecuteAlways]
public class InteriorCameraSizeFitter : MonoBehaviour
{
    [SerializeField]
    Camera cam;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Update()
    {
        cam.orthographicSize = 5.0f / (cam.aspect / (3.0f / 4.0f));
    }
}