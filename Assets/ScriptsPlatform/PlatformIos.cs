using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class PlatformIos : MonoBehaviour, IPlatformBase
{
    const string GameCenterLoginFailedFlagPrefKey = "__game_center_login_failed_flag";
    const string GameCenterLoginDisabledFlagPrefKey = "__game_center_login_disabled_flag";
#if UNITY_IOS
    static bool registerForNotificationsOnce;
#endif
    Action<byte[]> onPeekResultSave;

    [SerializeField]
    Platform platform;

    [SerializeField]
    PlatformSaveUtil platformSaveUtil;

    public bool CheckLoadSavePrecondition(string progressMessage, Action onNotLoggedIn, Action onAbort)
    {
        // Game Center 로그인 시도 회수 제한(3회 연속 로그인 거절)이 있다.
        // 회수 제한을 넘어서면 아무런 응답이 오지 않기 때문에 아예 시도조차 하지 않아야 한다.
        if (!PlatformLogin.IsAuthenticated && PlayerPrefs.GetInt(GameCenterLoginDisabledFlagPrefKey, 0) != 0)
        {
            // 유저가 직접 홈 -> 설정 -> Game Center 로그인을 해야 한다는 것을 알려야된다.
            PlatformInterface.Instance.confirmPopup.Open(
                PlatformInterface.Instance.textHelper.GetText("platform_game_center_login_required_popup"));
            return false;
        }

        if (!PlatformLogin.IsAuthenticated)
        {
            PlatformInterface.Instance.confirmPopup.OpenYesNoPopup(
                platform.GetText("platform_game_center_login_required_popup"),
                onNotLoggedIn, onAbort);
            return false;
        }

        // 여기까지 왔으면 Game Center 로그인은 성공한 상태란 뜻이다.
        // Game Center 로그인을 앞으로 다시 시도하도록 한다.
        PlayerPrefs.SetInt(GameCenterLoginDisabledFlagPrefKey, 0);
        PlayerPrefs.SetInt(GameCenterLoginFailedFlagPrefKey, 0);

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            PlatformInterface.Instance.confirmPopup.Open(
                PlatformInterface.Instance.textHelper.GetText("platform_load_require_internet_popup"));
            return false;
        }

        if (string.IsNullOrEmpty(progressMessage) == false)
            PlatformInterface.Instance.progressMessage.Open(progressMessage);

        return true;
    }

    public void GetCloudLastSavedMetadataAsync(Action<byte[]> onPeekResult)
    {
        if (!PlatformLogin.IsAuthenticated)
        {
            PlatformInterface.Instance.logger.LogFormat("GetCloudSavedAccountData: not authenticated");
            onPeekResult?.Invoke(null);

            return;
        }

        // 아래 함수의 호출 결과는 결과는 PlatformCallbackHandler GameObject의
        // PlatformCallbackHandler.OnIosLoadResult()로 비동기적으로 호출되는 것으로 처리한다.
        // 이를 위해 onPeekResult를 챙겨둔다.
        onPeekResultSave = onPeekResult;
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            PlatformIosNative.loadFromCloudPrivate(Social.localUser.id, PlatformInterface.Instance.text.LoginErrorTitle,
                PlatformInterface.Instance.text.LoginErrorMessage,
                PlatformInterface.Instance.text.ConfirmMessage);
    }

    public void ExecuteCloudLoad()
    {
        platformSaveUtil.ShowLoadProgressPopup();

        // GetCloudLastSavedMetadataAsync()의 첫 번째 인자가 null이면
        // 게임 데이터 로드로 작동한다.
        GetCloudLastSavedMetadataAsync(null);
    }

    public void ExecuteCloudSave()
    {
        PlatformInterface.Instance.saveLoadManager.SaveBeforeCloudSave();
        platformSaveUtil.ShowSaveProgressPopup();
#pragma warning disable 219
        var savedData = PlatformInterface.Instance.saveUtil.SerializeSaveData();
#pragma warning restore 219
        // 아래 함수의 호출 결과는 결과는 PlatformCallbackHandler GameObject의
        // PlatformCallbackHandler.OnIosSaveResult()로 비동기적으로 호출되는 것으로 처리한다.
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            PlatformIosNative.saveToCloudPrivate(Social.localUser.id, Convert.ToBase64String(savedData),
                PlatformInterface.Instance.text.LoginErrorTitle, PlatformInterface.Instance.text.LoginErrorMessage,
                PlatformInterface.Instance.text.ConfirmMessage);
    }

    public void RegisterAllNotifications(string title, string body, string largeIcon, int localHours)
    {
        throw new NotImplementedException();
    }

    public void RegisterSingleNotification(string title, string body, int afterMs, string largeIcon)
    {
        throw new NotImplementedException();
    }

    public async void Login(Action<bool, string> onAuthResult)
    {
        // iOS에서는 Social.localUser.Authenticate 콜백이 호출이 되지 않을 때도 있다.
        // (유저가 의도적으로 로그인을 3회 거절한 경우, 4회째부터는 콜백이 안온다. ㄷㄷ)
        // 6.0초 타임아웃을 재자.

        var authResultTask = await Task.WhenAny(Task.Run(async () =>
        {
            await Task.Delay(6000);
            return new Tuple<bool, string>(false, "TIMEOUT");
        }), Task.Run(async () =>
        {
            var authenticateTask = new TaskCompletionSource<Tuple<bool, string>>();
            await UnityMainThreadDispatcher.Instance().EnqueueAsync(() =>
            {
                Social.localUser.Authenticate((b, reason) =>
                {
                    authenticateTask.SetResult(new Tuple<bool, string>(b, reason));
                });
            });
            return await authenticateTask.Task;
        }));

        // 정상 반환이건 타임아웃이건 둘 중 하나에 대해서만 AuthenticateCallback()을 호출 해 준다.
        var authResult = await authResultTask;
        AuthenticateCallback(onAuthResult, authResult.Item1, authResult.Item2);
    }

    [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
    static void AuthenticateCallback(Action<bool, string> onAuthResult, bool b, string reason)
    {
        // Game Center 로그인 성공/실패 유무에 따른 플래그 업데이트
        PlayerPrefs.SetInt(GameCenterLoginFailedFlagPrefKey, b ? 0 : 1);

        PlatformInterface.Instance.logger.LogFormat("iOS Game Center Login Result: {0} / Reason: {1}", b, reason);

        // 회수 제한 마지막 기회임을 체크해서 다시는 시도하지 않도록 한다.
        // 그런데... reason은 유저가 읽을 수 있도록 시스템 언어 설정에 따라
        // 같은 결과라도 언어별로 값이 다르다. ㄷㄷㄷ
        // 그러므로 GAME_CENTER_LOGIN_DISABLED_FLAG_PREF_KEY 플래그를 올리는 것은
        // 제대로 작동하지 않는다. (언어별로 정상 작동 여부가 달라진다.)
        // 아직은 고치지 않고 그래두 두겠다...
        if (b == false && reason.Contains("canceled") && reason.Contains("disabled"))
            PlayerPrefs.SetInt(GameCenterLoginDisabledFlagPrefKey, 1);
        else if (b) PlayerPrefs.SetInt(GameCenterLoginDisabledFlagPrefKey, 0);

        PlayerPrefs.Save();

        onAuthResult(b, reason);

#if UNITY_IOS
        // 푸시 알림 관련한 기능 아직은 쓰지 않으니까 권한 요청하지 말자.
        /*
        if (registerForNotificationsOnce == false) {
            StartCoroutine(RequestAuthorization());
            registerForNotificationsOnce = true;
        }
        */

        // 로그인 성공했다면 업적 알림 기능 켜기
        if (b) {
            UnityEngine.SocialPlatforms.GameCenter.GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
        }
#endif
    }

    public bool LoginFailedLastTime()
    {
        // iOS는 마지막 로그인이 실패했건 안했건 무조건 구동 시 로그인 시도 한다.
        return false;
    }

    public void Logout()
    {
        PlatformInterface.Instance.logger.Log("PlatformIos.Logout()");
    }

    public void PreAuthenticate()
    {
    }

    public void Report(string reportPopupTitle, string mailTo, string subject, string text, byte[] saveData)
    {
        var reportSaveDataPath = Application.persistentDataPath + "/report-save-data";
        File.WriteAllBytes(reportSaveDataPath, saveData);
#if UNITY_IOS
        PlatformIosNative.sendMail(subject, text, mailTo, reportSaveDataPath);
#endif
    }

    public void ShareScreenshot(byte[] pngData)
    {
        var pngDataPath = Application.persistentDataPath + "/screenshot-share.png";
        File.WriteAllBytes(pngDataPath, pngData);
        NativeShare.Share("", pngDataPath, null, "", "image/png", true, "Share");
    }

    public void ClearAllNotifications()
    {
#if UNITY_IOS
        Debug.Log("PlatformIos.ClearAllNotifications()");
        PlatformIosNative.clearAllNotifications();
#endif
    }

    public void OnCloudSaveResult(string result)
    {
        if (result == "OK") // handle reading or writing of saved game.
            platformSaveUtil.ShowSaveResultPopup();
        else // handle error
            platformSaveUtil.ShowSaveErrorPopup(
                PlatformInterface.Instance.textHelper.GetText("platform_cloud_save_fail") +
                "\n\n" + result);
    }

    public void OnCloudLoadResult(string result, byte[] data)
    {
        if (result == "OK")
        {
            PlatformInterface.Instance.logger.LogFormat("OnCloudLoadResult: data length {0} bytes",
                data?.Length ?? 0);
            // 메타데이터 조회의 경우와 실제 세이브 데이터 로딩의 경우를 나눠서 처리
            if (onPeekResultSave != null)
            {
                PlatformInterface.Instance.logger.Log("OnCloudLoadResult: onPeekResultSave valid");
                onPeekResultSave(data);
                onPeekResultSave = null;
            }
            else
            {
                PlatformInterface.Instance.logger.Log("OnCloudLoadResult: onPeekResultSave empty. data load...");
                if (data == null || data.Length == 0)
                {
                    platformSaveUtil.ShowLoadErrorPopup("OnCloudLoadResult: Cloud save data corrupted");
                }
                else
                {
                    PlatformInterface.Instance.logger.LogFormat("OnCloudLoadResult: success! - Data size: {0} bytes",
                        data.Length);
                    var remoteSaveDict = PlatformInterface.Instance.saveUtil.DeserializeSaveData(data);
                    PlatformInterface.Instance.saveUtil.LoadDataAndLoadSplashScene(remoteSaveDict);
                }
            }
        }
        else
        {
            platformSaveUtil.ShowSaveErrorPopup(
                PlatformInterface.Instance.textHelper.GetText("platform_cloud_load_fail") +
                "\n\n" + result);
        }
    }

    public void RequestUserReview()
    {
        Application.OpenURL(PlatformInterface.Instance.config.GetUserReviewUrl());
    }

    public string GetAccountTypeText()
    {
        return PlatformInterface.Instance.textHelper.GetText("platform_account_game_center");
    }
}