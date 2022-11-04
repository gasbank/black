//source: https://forum.unity.com/threads/canvashelper-resizes-a-recttransform-to-iphone-xs-safe-area.521107

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Canvas))]
public class CanvasHelper : MonoBehaviour {
    static readonly List<CanvasHelper> Helpers = new();

    public static readonly UnityEvent OnResolutionOrOrientationChanged = new();

    static bool screenChangeVarsInitialized = false;
    static ScreenOrientation lastOrientation = ScreenOrientation.LandscapeLeft;
    static Vector2 lastResolution = Vector2.zero;
    static Rect lastSafeArea = Rect.zero;

    Canvas canvas;
    RectTransform safeAreaTransform;

    void Awake() {
        if (!Helpers.Contains(this)) {
            Helpers.Add(this);
        }

        canvas = GetComponent<Canvas>();
        GetComponent<RectTransform>();

        safeAreaTransform = transform.Find("SafeArea") as RectTransform;

        if (!screenChangeVarsInitialized) {
            lastOrientation = Screen.orientation;
            lastResolution.x = Screen.width;
            lastResolution.y = Screen.height;
            lastSafeArea = Screen.safeArea;

            screenChangeVarsInitialized = true;
        }

        ApplySafeArea();
    }

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    void Update() {
        if (Helpers[0] != this) {
            return;
        }

        if (Application.isMobilePlatform && Screen.orientation != lastOrientation) {
            OrientationChanged();
        }

        if (Screen.safeArea != lastSafeArea) {
            SafeAreaChanged();
        }

        if (Screen.width != lastResolution.x || Screen.height != lastResolution.y) {
            ResolutionChanged();
        }
    }

    void OnDestroy() {
        if (Helpers != null && Helpers.Contains(this)) {
            Helpers.Remove(this);
        }
    }

    void ApplySafeArea() {
        if (safeAreaTransform == null) {
            return;
        }

        Rect safeArea = Screen.safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        var pixelRect = canvas.pixelRect;
        anchorMin.x /= pixelRect.width;
        anchorMin.y /= pixelRect.height;
        anchorMax.x /= pixelRect.width;
        anchorMax.y /= pixelRect.height;

        safeAreaTransform.anchorMin = anchorMin;
        safeAreaTransform.anchorMax = anchorMax;
    }

    static void OrientationChanged() {
        //Debug.Log("Orientation changed from " + lastOrientation + " to " + Screen.orientation + " at " + Time.time);

        lastOrientation = Screen.orientation;
        lastResolution.x = Screen.width;
        lastResolution.y = Screen.height;

        OnResolutionOrOrientationChanged.Invoke();
    }

    static void ResolutionChanged() {
        //Debug.Log("Resolution changed from " + lastResolution + " to (" + Screen.width + ", " + Screen.height + ") at " + Time.time);

        lastResolution.x = Screen.width;
        lastResolution.y = Screen.height;

        OnResolutionOrOrientationChanged.Invoke();
    }

    static void SafeAreaChanged() {
        // Debug.Log("Safe Area changed from " + lastSafeArea + " to " + Screen.safeArea.size + " at " + Time.time);

        lastSafeArea = Screen.safeArea;

        foreach (var t in Helpers)
        {
            t.ApplySafeArea();
        }
    }
}