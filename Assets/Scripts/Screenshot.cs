using ConditionalDebug;
using UnityEngine;
using UnityEngine.InputSystem;

public class Screenshot : MonoBehaviour
{
    static readonly string filename = "screenshot.png";

    void Update()
    {
        if (Keyboard.current[Key.F8].wasReleasedThisFrame)
        {
            ScreenCapture.CaptureScreenshot(filename);
            ConDebug.Log($"Screenshot saved to {filename}");
        }
    }
}