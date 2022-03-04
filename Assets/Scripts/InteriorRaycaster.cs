using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.Serialization;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[DisallowMultipleComponent]
public class InteriorRaycaster : MonoBehaviour
{
    [SerializeField]
    Camera interiorCam;

    [SerializeField]
    Debris activeDebris;

    [FormerlySerializedAs("colliderMask")]
    [SerializeField]
    LayerMask fingerColliderMask;
    
    [SerializeField]
    LayerMask placementColliderMask;

    RaycastHit[] raycastHitList;
    
#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Start()
    {
        raycastHitList = new RaycastHit[10];
    }

    protected void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable();
        
        Touch.onFingerDown += OnFingerDown;
        Touch.onFingerMove += OnFingerMove;
    }

    protected void OnDisable()
    {
        EnhancedTouchSupport.Disable();

        Touch.onFingerDown -= OnFingerDown;
        Touch.onFingerMove -= OnFingerMove;
    }

    void OnFingerDown(Finger finger)
    {
        UpdateActiveDebrisPosition(finger);
    }

    void UpdateActiveDebrisPosition(Finger finger)
    {
        var ray = interiorCam.ScreenPointToRay(finger.screenPosition);
        var fingerHitCount = Physics.RaycastNonAlloc(ray, raycastHitList, 1000.0f, fingerColliderMask);

        var debrisBoxCollider = activeDebris.BoxCollider;
        //debrisBoxCollider.center
        //activeDebris.BoxColliderGlobalScale
        //transform.loss
        //debrisBoxCollider.
        var debrisTransform = debrisBoxCollider.transform;
        var globalCenter = debrisTransform.TransformPoint(debrisBoxCollider.center);
        var globalExtents = Vector3.Scale(debrisBoxCollider.size, debrisTransform.lossyScale) / 2;

        if (fingerHitCount <= 0)
        {
            return;
        }
        
        var h = raycastHitList[0];
        
        Debug.Log($"Hit: {h.collider.name}");
        Debug.Log($"Hit Point: {h.point}");

        var wallCollider = h.transform.GetComponentInChildren<QuadCollider>();
        if (wallCollider == null)
        {
            return;
        }

        var worldSize = debrisBoxCollider.transform.TransformDirection(debrisBoxCollider.size);
        worldSize.x = Mathf.Abs(worldSize.x);
        worldSize.y = Mathf.Abs(worldSize.y);
        worldSize.z = Mathf.Abs(worldSize.z);
        worldSize = Vector3.Scale(debrisBoxCollider.transform.lossyScale, worldSize);
        Debug.Log($"Debris World Size: {worldSize}");

        var finalPlacementPoint = wallCollider.GetClampedWorldPoint(worldSize, h.point);
            
        activeDebris.MoveByScreenPoint(finalPlacementPoint + h.normal, finalPlacementPoint, RectTransformUtility.WorldToScreenPoint(interiorCam, finalPlacementPoint));
        //Debug.Log(h.transform.name);
    }

    void OnFingerMove(Finger finger)
    {
        UpdateActiveDebrisPosition(finger);
    }
}