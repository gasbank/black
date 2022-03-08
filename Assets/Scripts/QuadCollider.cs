using UnityEngine;

[DisallowMultipleComponent]
public class QuadCollider : MonoBehaviour
{
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    public Vector3 GetClampedWorldPoint(Vector3 worldSize, Vector3 worldPoint)
    {
        var localSize = transform.InverseTransformDirection(worldSize);
        
        Debug.Log($"Quad Collider Local Size: {localSize}", this);
        
        var globalScale = transform.lossyScale;
        
        Debug.Log($"Quad Collider Global Scale: {globalScale}", this);
        
        localSize.x /= globalScale.x;
        localSize.y /= globalScale.y;
        localSize.z /= globalScale.z;

        var center = Vector3.zero;
        var size = Vector3.one;
        
        var localCornerMin = center - size / 2 + localSize / 2;
        var localCornerMax = center + size / 2 - localSize / 2;
        
        var localPoint = transform.InverseTransformPoint(worldPoint);

        localPoint.x = Mathf.Clamp(localPoint.x, localCornerMin.x, localCornerMax.x);
        localPoint.y = Mathf.Clamp(localPoint.y, localCornerMin.y, localCornerMax.y);
        //localPoint.z = Mathf.Clamp(localPoint.z, localCornerMin.z, localCornerMax.z);

        return transform.TransformPoint(localPoint);
    }
}