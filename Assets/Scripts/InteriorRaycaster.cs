using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[DisallowMultipleComponent]
public class InteriorRaycaster : MonoBehaviour
{
    [SerializeField]
    Camera interiorCam;

    [SerializeField]
    Debris activeDebris;

    [SerializeField]
    LayerMask colliderMask;

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
        var hitCount = Physics.RaycastNonAlloc(ray, raycastHitList, 1000.0f, colliderMask);
        for (var i = 0; i < Mathf.Min(1, hitCount); i++)
        {
            var h = raycastHitList[i];
            activeDebris.MoveByScreenPoint(h.point + h.normal, h.point, RectTransformUtility.WorldToScreenPoint(interiorCam, h.point));
            //Debug.Log(h.transform.name);
        }
    }

    void OnFingerMove(Finger finger)
    {
        UpdateActiveDebrisPosition(finger);
    }
}