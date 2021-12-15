using UnityEngine;

public class Screenshot : MonoBehaviour
{
    static readonly string filename = "screenshot.png";

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F8))
        {
            ScreenCapture.CaptureScreenshot(filename);
            Debug.Log($"Screenshot saved to {filename}");
        }
    }
}