using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlatformLogin : MonoBehaviour
{
    bool atLeastTriedOnce = false;
    bool? authenticatedBeforePause;

    [SerializeField]
    Platform platform;

    [SerializeField]
    PlatformInterface platformInterface;

    [SerializeField]
    CanvasGroup rootCanvasGroup = null;

    [SerializeField]
    Text userId = null;

    [SerializeField]
    Text userName = null;

    public static bool IsAuthenticated => Social.localUser != null && Social.localUser.authenticated;

    void Start()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer) // 여유 주지 않으면 iOS에서 크래시나기 시작했다?
            StartCoroutine(StartLoginAfter5Seconds());
        else
            StartLogin();
    }

    IEnumerator StartLoginAfter5Seconds()
    {
        yield return new WaitForSeconds(5.0f);
        StartLogin();
    }

    void StartLogin()
    {
        PlatformInterface.instance.logger.Log("PlatformLogin.StartLogin()");
        if (platform.DisableLoginOnStart)
        {
            PlatformInterface.instance.logger.Log(
                "PlatformLogin: Skipping StartLogin() since DisableLoginOnStart is true.");
            return;
        }

        //rootCanvasGroup.interactable = false;
        try
        {
            platform.StartAuthAsync((result, reason) =>
            {
                if (rootCanvasGroup != null) rootCanvasGroup.interactable = true;
                PlatformInterface.instance.logger.LogFormat("PlatformLogin: Social.localUser.Authenticate {0}", result);
                if (result)
                {
                    if (Social.localUser != null)
                    {
                        PlatformInterface.instance.logger.LogFormat(
                            "PlatformLogin: Social.localUser userName={0}, userId={1}", Social.localUser.userName,
                            Social.localUser.id);
                        if (userName) userName.text = Social.localUser.userName;
                        if (userId) userId.text = Social.localUser.id;
                    }
                    else
                    {
                        Debug.LogError("PlatformLogin: Login result is true, but Social.localUser is null!");
                    }
                }

                atLeastTriedOnce = true;
            });
        }
        catch (Exception e)
        {
            PlatformInterface.instance.logger.Log($"PlatformLogin.StartLogin() - exception: {e}");
            if (rootCanvasGroup != null) rootCanvasGroup.interactable = true;
            atLeastTriedOnce = true;
        }
    }

    void OnApplicationPause(bool pause)
    {
        PlatformInterface.instance.logger.Log($"PlatformLogin.OnApplicationPause({pause})");
        if (pause)
            if (atLeastTriedOnce && IsAuthenticated == false)
            {
                PlatformInterface.instance.logger.Log(
                    "PlatformLogin: Not authenticated even if we tried. User probably logged out from service.");
                platform.DisableLoginOnStart = true;
            }
    }
}