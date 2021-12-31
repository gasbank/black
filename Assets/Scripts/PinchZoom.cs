using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class PinchZoom : MonoBehaviour
{
    [SerializeField]
    MainGame mainGame;

    [SerializeField]
    float maxScale = 5.0f;

    [SerializeField]
    float minScale = 0.5f;

    [SerializeField]
    Transform targetImage;

    [SerializeField]
    Slider zoomSlider;

    public static bool PinchZooming => Touch.activeTouches.Count == 2;

    public float ZoomValue
    {
        get => zoomSlider.value;
        set => zoomSlider.value = value;
    }

    void Update()
    {
        // If there are two touches on the device...
        
        if (Touch.activeTouches.Count == 2 && mainGame.CanInteractPanAndZoom)
        {
            // Store both touches.
            
            var touchZero = Touch.activeTouches[0];
            var touchOne = Touch.activeTouches[1];

            // Find the position in the previous frame of each touch.
            var touchZeroPrevPos = touchZero.screenPosition - touchZero.delta;
            var touchOnePrevPos = touchOne.screenPosition - touchOne.delta;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            var prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            var touchDeltaMag = (touchZero.screenPosition - touchOne.screenPosition).magnitude;

            // Find the difference in the distances between each frame.
            var deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // Slider 콜백을 유도한다.
            ZoomValue = Mathf.Clamp(ZoomValue - deltaMagnitudeDiff / 200.0f, minScale, maxScale);
        }
    }

    // Slider 콜백으로 호출된다.
    public void Zoom(float scale)
    {
        var newScale = Mathf.Clamp(scale, minScale, maxScale);
        targetImage.localScale = Vector3.one * newScale;
    }

    public void ResetZoom()
    {
        ZoomValue = minScale;
    }
}