using UnityEngine;

[DisallowMultipleComponent]
public class WallCollider : MonoBehaviour
{
    [SerializeField]
    BoxCollider boxCollider;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    public Vector3 GetClampedWorldPoint(Vector3 worldSize, Vector3 worldPoint)
    {
        var localSize = transform.InverseTransformDirection(worldSize);
        var globalScale = transform.lossyScale;
        localSize.x /= globalScale.x;
        localSize.y /= globalScale.y;
        localSize.z /= globalScale.z;
        
        var localCornerMin = boxCollider.center - boxCollider.size / 2 + localSize / 2;
        var localCornerMax = boxCollider.center + boxCollider.size / 2 - localSize / 2;
        
        var localPoint = transform.InverseTransformPoint(worldPoint);

        localPoint.x = Mathf.Clamp(localPoint.x, localCornerMin.x, localCornerMax.x);
        localPoint.y = Mathf.Clamp(localPoint.y, localCornerMin.y, localCornerMax.y);
        localPoint.z = Mathf.Clamp(localPoint.z, localCornerMin.z, localCornerMax.z);

        return transform.TransformPoint(localPoint);
    }
}