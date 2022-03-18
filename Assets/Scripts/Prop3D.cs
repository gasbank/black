using System;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class Prop3D : MonoBehaviour, IWorldPosition3D
{
    static bool Verbose => false;
    
    [FormerlySerializedAs("placedProp")]
    [FormerlySerializedAs("museumDebris")]
    [SerializeField]
    Prop prop;

    [SerializeField]
    BoxCollider boxCollider;

    [FormerlySerializedAs("attachLayer")]
    [SerializeField]
    LayerMask attachRaycastLayer;

    [SerializeField]
    Camera interiorCam;

    int overlappedCount;
    
    public BoxCollider BoxCollider => boxCollider;

    public LayerMask AttachRaycastLayer => attachRaycastLayer;

    public bool Overlapped => overlappedCount > 0;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    [ContextMenu("Apply To Prop")]
    public void ApplyToProp()
    {
        var position = transform.position;
        var screenPoint = RectTransformUtility.WorldToScreenPoint(interiorCam, position);
        MoveByScreenPoint(position + transform.forward, position, screenPoint);
    }
    
    public void MoveByScreenPoint(Vector3 forwardPoint, Vector3 worldPoint, Vector2 screenPoint)
    {
        if (transform == null)
        {
            return;
        }
        
        transform.position = worldPoint;
        transform.LookAt(forwardPoint);
        var forward = forwardPoint - worldPoint;
        var facingDeg = Mathf.Atan2(forward.z, forward.x) * Mathf.Rad2Deg;

        if (Verbose)
        {
            Debug.Log($"Facing Angle Deg: {facingDeg}");
        }

        prop.MoveByScreenPoint(screenPoint, facingDeg > 90 + 45 && facingDeg < 90 + 45 + 90);
        if (ActivePropButtonGroup.Instance != null)
        {
            ActivePropButtonGroup.Instance.HideButtonGroup();
        }
    }

    public void SetSiblingIndexFor2D(int index)
    {
        prop.transform.SetSiblingIndex(index);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Collided with {other.name}", other);
        var attachedRigidbody = other.attachedRigidbody;
        if (attachedRigidbody)
        {
            Debug.Log($"... Attached Rigid Body Name: {attachedRigidbody.name}", attachedRigidbody);
        }

        overlappedCount++;
        if (overlappedCount < 0)
        {
            Debug.LogError("Overlapped count is negative.");
        }
    }

    void OnTriggerExit(Collider other)
    {   
        overlappedCount--;
        if (overlappedCount < 0)
        {
            Debug.LogError("Overlapped count is negative.");
        }
        
        Debug.Log($"Un-collided with {other.name}", other);
        var attachedRigidbody = other.attachedRigidbody;
        if (attachedRigidbody)
        {
            Debug.Log($"... Attached Rigid Body Name: {attachedRigidbody.name}", attachedRigidbody);
        }
    }

    public Vector3 WorldPosition3D => transform.position;
}