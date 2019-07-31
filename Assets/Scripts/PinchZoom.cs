using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PinchZoom : MonoBehaviour {
    [SerializeField] Transform targetImage = null;
    [SerializeField] float minScale = 0.5f;
    [SerializeField] float maxScale = 5.0f;
    [SerializeField] Slider zoomSlider = null;
    
    public static bool PinchZooming => Input.touchCount == 2;
    public float ZoomValue { get => zoomSlider.value; set => zoomSlider.value = value; }

    void Update() {
        // If there are two touches on the device...
        if (Input.touchCount == 2) {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // Slider 콜백을 유도한다.
            ZoomValue = Mathf.Clamp(ZoomValue - deltaMagnitudeDiff / 200.0f, minScale, maxScale);
        }
    }

    // Slider 콜백으로 호출된다.
    public void Zoom(float scale) {
        var newScale = Mathf.Clamp(scale, minScale, maxScale);
        targetImage.localScale = Vector3.one * newScale;
    }

    public void ResetZoom() {
        ZoomValue = minScale;
    }
}
