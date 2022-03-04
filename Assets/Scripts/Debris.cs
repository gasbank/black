using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Debris : MonoBehaviour
{
    [SerializeField]
    MuseumDebris museumDebris;

    [SerializeField]
    BoxCollider boxCollider;

    public BoxCollider BoxCollider => boxCollider;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif
    public void MoveByScreenPoint(Vector3 forward, Vector3 worldPoint, Vector2 screenPoint)
    {
        if (transform == null)
        {
            return;
        }
        
        transform.position = worldPoint;
        transform.LookAt(forward);
        museumDebris.MoveByScreenPoint(screenPoint);
    }
}