using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class PinchZoom : MonoBehaviour
{
    [SerializeField]
    float maxScale = 7.0f;

    [SerializeField]
    float minScale = 1.0f;

    [SerializeField]
    Transform targetImage;

    [SerializeField]
    Slider zoomSlider;

    [SerializeField]
    float sensitivity = 0.001f;

    [SerializeField]
    AdminButtonGroup adminButtonGroup;

    float lastMultiTouchDistance;

    public static bool PinchZooming => Touch.activeTouches.Count == 2;

    public float ZoomValue
    {
        get => zoomSlider.value;
        set => zoomSlider.value = value;
    }

    void Start()
    {
        EnhancedTouchSupport.Enable();
    }

    void Update()
    {
        if (Touch.activeFingers.Count == 2)
        {
            ZoomCamera(Touch.activeTouches[0], Touch.activeTouches[1]);
        }
    }

    // https://github.com/Yecats/GameDevTutorials
    void ZoomCamera(Touch firstTouch, Touch secondTouch)
    {
        if (firstTouch.phase == TouchPhase.Began || secondTouch.phase == TouchPhase.Began)
        {
            lastMultiTouchDistance = Vector2.Distance(firstTouch.screenPosition, secondTouch.screenPosition);
        }

        // Ensure that remaining logic only executes if either finger is actively moving
        if (firstTouch.phase != TouchPhase.Moved || secondTouch.phase != TouchPhase.Moved)
        {
            return;
        }

        //Calculate if fingers are pinching together or apart
        var newMultiTouchDistance = Vector2.Distance(firstTouch.screenPosition, secondTouch.screenPosition);

        // Find the difference in the distances between each frame.
        var deltaMagnitudeDiff = newMultiTouchDistance - lastMultiTouchDistance;

        // Slider 콜백을 유도한다.
        ZoomValue = Mathf.Clamp(ZoomValue + deltaMagnitudeDiff * sensitivity, minScale, maxScale);

        // Set the last distance calculation
        lastMultiTouchDistance = newMultiTouchDistance;
    }

    // Slider 콜백으로 호출된다.
    public void Zoom(float scale)
    {
        var newScale = Mathf.Clamp(scale, minScale, maxScale);
        targetImage.localScale = newScale * newScale * Vector3.one;
    }

    public void ResetZoom()
    {
        ZoomValue = minScale;
    }

    public void OpenSensitivityPopup()
    {
        adminButtonGroup.Close();
        ConfirmPopup.instance.OpenInputFieldPopup("Pinch Zoom Sensitivity", () =>
            {
                float.TryParse(ConfirmPopup.instance.InputFieldText, out sensitivity);
                ConfirmPopup.instance.Close();
            }, ConfirmPopup.instance.Close, "Configure", Header.Normal,
            sensitivity.ToString(CultureInfo.InvariantCulture),
            string.Empty);
    }
}