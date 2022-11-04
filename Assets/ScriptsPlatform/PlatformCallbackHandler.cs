using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlatformCallbackHandler : MonoBehaviour
{
    [SerializeField]
    PlatformInterface platformInterface;

    // Unity API의 한계로 함수 인자는 string 하나만 쓸 수 있다.
    void OnIosSaveResult(string result)
    {
        PlatformInterface.Instance.logger.LogFormat("PlatformCallbackHandler.OnIosSaveResult: {0}",
            result != null ? result : "(null)");
        Platform.Instance.OnCloudSaveResult(result);
    }

    // Unity API의 한계로 함수 인자는 string 하나만 쓸 수 있다.
    void OnIosLoadResult(string result)
    {
        if (result.StartsWith("*****ERROR***** "))
        {
            Platform.Instance.OnCloudLoadResult(result, null);
        }
        else
        {
            PlatformInterface.Instance.logger.LogFormat("PlatformCallbackHandler.OnIosLoadResult: {0}",
                result != null ? result : "(null)");
            byte[] loadedDataBytes = null;
            if (result != null)
                try
                {
                    loadedDataBytes = Convert.FromBase64String(result);
                }
                catch
                {
                    loadedDataBytes = null;
                }

            Platform.Instance.OnCloudLoadResult("OK", loadedDataBytes);
        }
    }
}