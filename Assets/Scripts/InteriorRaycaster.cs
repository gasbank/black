using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
//using UnityEngine.Serialization;
//using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[DisallowMultipleComponent]
public class InteriorRaycaster : MonoBehaviour
{
    public static InteriorRaycaster Instance;

    static bool Verbose => false;
    
    [SerializeField]
    Camera interiorCam;

    RaycastHit[] raycastHitList;
    
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

    void Start()
    {
        raycastHitList = new RaycastHit[10];
    }

//    protected void OnEnable()
//    {
//        EnhancedTouchSupport.Enable();
//        TouchSimulation.Enable();
//        
//        Touch.onFingerDown += OnFingerDown;
//        Touch.onFingerMove += OnFingerMove;
//    }
//
//    protected void OnDisable()
//    {
//        EnhancedTouchSupport.Disable();
//
//        Touch.onFingerDown -= OnFingerDown;
//        Touch.onFingerMove -= OnFingerMove;
//    }

    void OnFingerDown(Finger finger)
    {
        UpdateActivePropPosition(finger.screenPosition);
    }

    public void UpdateActivePropPosition(Vector2 screenPosition)
    {
        var prop = ActivePropButtonGroup.Instance.ActiveProp;
        if (prop == null)
        {
            return;
        }

        if (prop.Prop3D == null)
        {
            return;
        }
        
        var ray = interiorCam.ScreenPointToRay(screenPosition);
        var fingerHitCount = Physics.RaycastNonAlloc(ray, raycastHitList, 1000.0f, prop.Prop3D.AttachRaycastLayer);

        var propBoxCollider = prop.Prop3D.BoxCollider;

        if (fingerHitCount <= 0)
        {
            return;
        }
        
        var h = raycastHitList[0];

        if (Verbose)
        {
            Debug.Log($"Hit: {h.collider.name}");
            Debug.Log($"Hit Point: {h.point.ToString("F4")}");
        }

        var wallCollider = h.transform.GetComponentInChildren<QuadCollider>();
        if (wallCollider == null)
        {
            return;
        }

        if (Verbose)
        {
            Debug.Log($"propBoxCollider.size = {propBoxCollider.size.ToString("F4")}");
        }

        var colliderLocalSize = propBoxCollider.size;
        var colliderGlobalSize = Vector3.Scale(propBoxCollider.transform.lossyScale, colliderLocalSize);

        var worldSize = propBoxCollider.transform.TransformDirection(colliderGlobalSize);
        worldSize.x = Mathf.Abs(worldSize.x);
        worldSize.y = Mathf.Abs(worldSize.y);
        worldSize.z = Mathf.Abs(worldSize.z);

        if (Verbose)
        {
            Debug.Log($"Prop World Size: {worldSize.ToString("F4")}");
        }

        var finalPlacementPoint = wallCollider.GetClampedWorldPoint(worldSize, h.point);

        UpdateActivePropPositionBy3D(prop, finalPlacementPoint + h.normal, finalPlacementPoint);
    }

    public void UpdateActivePropPositionBy3D(Prop prop, Vector3 forwardPoint, Vector3 worldPoint)
    {
        if (prop == null)
        {
            return;
        }

        if (prop.Prop3D == null)
        {
            return;
        }
        
        prop.Prop3D.MoveByScreenPoint(forwardPoint, worldPoint, RectTransformUtility.WorldToScreenPoint(interiorCam, worldPoint));
    }

    void OnFingerMove(Finger finger)
    {
        UpdateActivePropPosition(finger.screenPosition);
    }
}