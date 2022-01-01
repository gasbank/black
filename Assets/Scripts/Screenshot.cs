using ConditionalDebug;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Screenshot : MonoBehaviour
{
    static readonly string filename = "screenshot.png";

    [SerializeField]
    bool scanOnce;

    void Update()
    {
        if (Keyboard.current[Key.F8].wasReleasedThisFrame)
        {
            if (scanOnce == false)
            {
                scanOnce = true;

                foreach (var rootGo in SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    foreach (var comp in rootGo.GetComponentsInChildren<DeactivateOnLiveBuild>())
                    {
                        comp.gameObject.SetActive(false);
                    }
                    
                    foreach (var comp in rootGo.GetComponentsInChildren<HideOnLiveBuild>())
                    {
                        comp.Hide();
                    }
                }
            }

            ScreenCapture.CaptureScreenshot(filename);
            ConDebug.Log($"Screenshot saved to {filename}");
        }
    }
}