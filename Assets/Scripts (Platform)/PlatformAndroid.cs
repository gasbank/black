using System;
#if !NO_GPGS
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
#endif
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif
using UnityEngine;

public class PlatformAndroid : MonoBehaviour, IPlatformBase {
    [SerializeField]
    Platform platform;
    
    [SerializeField]
    PlatformInterface platformInterface;

    [SerializeField]
    PlatformSaveUtil platformSaveUtil;
    
    static string GOOGLE_LOGIN_FAILED_FLAG_PREF_KEY = "__google_login_failed_flag";
    public static bool OnSavedGameOpenedAndWriteAlwaysInternalError = false;
#if UNITY_ANDROID
    string NOTIFICATION_MANAGER_FULL_CLASS_NAME => PlatformInterface.instance.config.NotificationManagerFullClassName;
    // 버그 신고 기능과 스크린샷 기능은 기능상은 다르지만, 라이브러리를 따로 추가하지
    // 않고 구현했으므로 같은 이름을 쓴다.
    string SCREENSHOT_AND_REPORT_FULL_CLASS_NAME => PlatformInterface.instance.config.ScreenshotAndReportFullClassName;
#endif
    public bool CheckLoadSavePrecondition(string progressMessage, Action onNotLoggedIn, Action onAbort) {
        if (!PlatformLogin.IsAuthenticated) {
            PlatformInterface.instance.confirmPopup.OpenYesNoPopup(platform.GetText("platform_google_login_required_popup"), onNotLoggedIn, onAbort);
            return false;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable) {
            PlatformInterface.instance.confirmPopup.Open(PlatformInterface.instance.textHelper.GetText("platform_load_require_internet_popup"));
            return false;
        }

        if (string.IsNullOrEmpty(progressMessage) == false) {
            PlatformInterface.instance.progressMessage.Open(progressMessage);
        }
        return true;
    }

#if !NO_GPGS
    void Open(ISavedGameClient savedGameClient, bool useAutomaticResolution, ConflictCallback conflictCallback, System.Action<SavedGameRequestStatus, ISavedGameMetadata> completedCallback) {

        if (useAutomaticResolution) {
            savedGameClient.OpenWithAutomaticConflictResolution(
                PlatformSaveUtil.remoteSaveFileName,
                DataSource.ReadNetworkOnly,
                ConflictResolutionStrategy.UseLongestPlaytime,
                completedCallback);
        } else {
            savedGameClient.OpenWithManualConflictResolution(
                PlatformSaveUtil.remoteSaveFileName,
                DataSource.ReadNetworkOnly,
                true,
                conflictCallback,
                completedCallback);
        }
    }
#endif
    
    public void GetCloudLastSavedMetadataAsync(Action<byte[]> onPeekResult) {
#if !NO_GPGS
        if (!PlatformLogin.IsAuthenticated) {
            PlatformInterface.instance.logger.LogFormat("GetCloudSavedAccountData: not authenticated");
            onPeekResult(null);
            return;
        }

        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        if (savedGameClient != null) {
            Open(savedGameClient,
                true,
                OnSavedGameOpenedAndReadConflictResolve,
                (status, game) => {
                    if (status == SavedGameRequestStatus.Success) {
                        // handle reading or writing of saved game.

                        PlatformInterface.instance.logger.LogFormat("GetCloudSavedAccountData: Save game open (read) success! Filename: {0}", game.Filename);

                        savedGameClient.ReadBinaryData(game, (status2, data2) => {
                            if (status == SavedGameRequestStatus.Success) {
                                // handle processing the byte array data
                                PlatformInterface.instance.logger.LogFormat("GetCloudSavedAccountData success! - Data size: {0} bytes", data2.Length);
                                try {
                                    onPeekResult(data2);
                                } catch {
                                    PlatformInterface.instance.logger.LogFormat("GetCloudSavedAccountData: Exception at deserialization");
                                    onPeekResult(null);
                                }
                            } else {
                                PlatformInterface.instance.logger.LogFormat("GetCloudSavedAccountData: ReadBinaryData error! - {0}", status2);
                                onPeekResult(null);
                            }
                        });
                    } else {
                        platformSaveUtil.LogCloudLoadSaveError(string.Format("GetCloudSavedAccountData: OpenWithAutomaticConflictResolution error! - {0}", status));
                        onPeekResult(null);
                    }
                });
        } else {
            platformSaveUtil.LogCloudLoadSaveError(string.Format("GetCloudSavedAccountData: savedGameClient null"));
            onPeekResult(null);
        }
#endif
    }

    public void ExecuteCloudLoad() {
#if !NO_GPGS
        platformSaveUtil.ShowLoadProgressPopup();

        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        if (savedGameClient != null) {
            Open(savedGameClient,
                true,
                OnSavedGameOpenedAndReadConflictResolve,
                OnSavedGameOpenedAndRead);
        } else {
            // handle error
            platformSaveUtil.ShowLoadErrorPopup("OnClick_cloudSave: savedGameClient null");
            PlatformInterface.instance.logManager.Add(PlatformInterface.instance.logEntryType.GameCloudLoadFailure, 0, 2);
        }
#endif
    }

    public void ExecuteCloudSave() {
#if !NO_GPGS
        PlatformInterface.instance.saveLoadManager.SaveBeforeCloudSave();
        platformSaveUtil.ShowSaveProgressPopup();

        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        if (savedGameClient != null) {
            Open(savedGameClient,
                true,
                OnSavedGameOpenedAndWriteConflictResolve,
                OnSavedGameOpenedAndWrite);
        } else {
            platformSaveUtil.ShowSaveErrorPopup("OnClick_cloudSave: savedGameClient null");
            PlatformInterface.instance.logManager.Add(PlatformInterface.instance.logEntryType.GameCloudSaveFailure, 0, 1);
        }
#endif
    }

    public void Login(Action<bool, string> onAuthResult) {
        Social.localUser.Authenticate((b, reason) => {
            // 구글 로그인 성공/실패 유무에 따른 플래그 업데이트
            PlayerPrefs.SetInt(GOOGLE_LOGIN_FAILED_FLAG_PREF_KEY, b ? 0 : 1);
            PlayerPrefs.Save();
            onAuthResult(b, reason);
        });
    }

    public void Logout() {
        PlatformInterface.instance.logger.Log("PlatformAndroid.Logout()");
#if !NO_GPGS
        PlayGamesPlatform.Instance.SignOut();
        platform.DisableLoginOnStart = true;
#endif
    }

    public void Report(string reportPopupTitle, string mailTo, string subject, string text, byte[] saveData) {
#if UNITY_ANDROID
        AndroidJavaClass pluginClass = new AndroidJavaClass(SCREENSHOT_AND_REPORT_FULL_CLASS_NAME);
        if (pluginClass != null) {
            pluginClass.CallStatic("ReportBugByMailSaveFileOnUiThread", reportPopupTitle, mailTo, subject, text, saveData);
        } else {
            Debug.LogErrorFormat("ReportBugByMailSaveFileOnUiThread: AndroidJavaClass name {0} not found!", SCREENSHOT_AND_REPORT_FULL_CLASS_NAME);
        }
#endif
    }

    public void ShareScreenshot(byte[] pngData) {
#if UNITY_ANDROID
        AndroidJavaClass pluginClass = new AndroidJavaClass(SCREENSHOT_AND_REPORT_FULL_CLASS_NAME);
        if (pluginClass != null) {
            pluginClass.CallStatic("SharePngByteArrayOnUiThread", pngData);
        } else {
            Debug.LogErrorFormat("SharePngByteArrayOnUiThread: AndroidJavaClass name {0} not found!", SCREENSHOT_AND_REPORT_FULL_CLASS_NAME);
        }
#endif
    }

#if !NO_GPGS
    private void OnSavedGameOpenedAndWriteConflictResolve(IConflictResolver resolver, ISavedGameMetadata original, byte[] originalData, ISavedGameMetadata unmerged, byte[] unmergedData) {
        resolver.ChooseMetadata(unmerged);
    }

    private void OnSavedGameOpenedAndReadConflictResolve(IConflictResolver resolver, ISavedGameMetadata original, byte[] originalData, ISavedGameMetadata unmerged, byte[] unmergedData) {
        resolver.ChooseMetadata(original);
    }
    
    public void OnSavedGameOpenedAndWrite(SavedGameRequestStatus status, ISavedGameMetadata game) {
        // 코너 케이스 테스트를 위한 코드
        if (OnSavedGameOpenedAndWriteAlwaysInternalError) {
            status = SavedGameRequestStatus.InternalError;
        }

        if (status == SavedGameRequestStatus.Success) {
            // handle reading or writing of saved game.

            PlatformInterface.instance.logger.LogFormat("OnSavedGameOpenedAndWrite: Save game open (write) success! Filename: {0}", game.Filename);

            SerializeAndSaveGame(game);
        } else {
            // handle error
            if (status == SavedGameRequestStatus.InternalError) {
                // Google Play 게임 앱 버전이 낮아서 InternalError가 나는 사례도 두 번 정도 제보되었다.
                // 여기서 관련된 정보를 추가로 알려주면 어쩌면 좋을지도...?
                platformSaveUtil.ShowSaveErrorPopupWithGooglePlayGamesUpdateButton(string.Format("OnSavedGameOpenedAndWrite: Save game open (write) failed! - {0}\nPlease consider updating Google Play Games app.", status));
            } else {
                platformSaveUtil.ShowSaveErrorPopup(string.Format("OnSavedGameOpenedAndWrite: Save game open (write) failed! - {0}", status));
            }
            //rootCanvasGroup.interactable = true;
            PlatformInterface.instance.logManager.Add(PlatformInterface.instance.logEntryType.GameCloudSaveFailure, 0, 2);
        }
    }
    void SerializeAndSaveGame(ISavedGameMetadata game) {
        var savedData = PlatformInterface.instance.saveUtil.SerializeSaveData();
        var played = PlatformInterface.instance.saveUtil.GetPlayed();
        var desc = PlatformInterface.instance.saveUtil.GetDesc(savedData);
        SaveGame(game, savedData, played, desc);
    }
#endif

#if !NO_GPGS
    void SaveGame(ISavedGameMetadata game, byte[] savedData, System.TimeSpan totalPlaytime, string desc) {

        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
        builder = builder.WithUpdatedDescription(desc);
        builder = builder.WithUpdatedPlayedTime(totalPlaytime);
        SavedGameMetadataUpdate updatedMetadata = builder.Build();
        savedGameClient.CommitUpdate(game, updatedMetadata, savedData, OnSavedGameWritten);
    }

    void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata game) {
        if (status == SavedGameRequestStatus.Success) {
            // handle reading or writing of saved game.
            platformSaveUtil.ShowSaveResultPopup();
        } else {
            // handle error
            platformSaveUtil.ShowSaveErrorPopup(string.Format("OnSavedGameWritten: OnSavedGameWritten failed! - {0}", status));
            PlatformInterface.instance.logManager.Add(PlatformInterface.instance.logEntryType.GameCloudSaveFailure, 0, 3);
        }

        //rootCanvasGroup.interactable = true;
    }

    public void OnSavedGameOpenedAndRead(SavedGameRequestStatus status, ISavedGameMetadata game) {
        if (status == SavedGameRequestStatus.Success) {
            // handle reading or writing of saved game.

            PlatformInterface.instance.logger.LogFormat("Save game open (read) success! Filename: {0}", game.Filename);

            LoadGameData(game);
        } else {
            // handle error
            platformSaveUtil.ShowLoadErrorPopup("OnSavedGameOpenedAndRead: status != SavedGameRequestStatus.Success");
            PlatformInterface.instance.logManager.Add(PlatformInterface.instance.logEntryType.GameCloudLoadFailure, 0, 3);
        }
    }
#endif

#if !NO_GPGS
    void LoadGameData(ISavedGameMetadata game) {

        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.ReadBinaryData(game, OnSavedGameDataRead);
    }
    
    void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] data) {
        if (status == SavedGameRequestStatus.Success) {
            // handle processing the byte array data
            PlatformInterface.instance.logger.LogFormat("OnSavedGameDataRead success! - Data size: {0} bytes", data.Length);

            var remoteSaveDict = PlatformInterface.instance.saveUtil.DeserializeSaveData(data);

            PlatformInterface.instance.saveUtil.LoadDataAndLoadSplashScene(remoteSaveDict);
        } else {
            // handle error
            platformSaveUtil.ShowLoadErrorPopup("OnSavedGameDataRead: status == SavedGameRequestStatus.Success");
            PlatformInterface.instance.logManager.Add(PlatformInterface.instance.logEntryType.GameCloudLoadFailure, 0, 4);
        }
    }
#endif
    
    public void PreAuthenticate() {
#if !NO_GPGS
        var config = new PlayGamesClientConfiguration.Builder()
                    .EnableSavedGames()
                    .Build();

        PlayGamesPlatform.InitializeInstance(config);
        //PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
#endif
        
#if UNITY_ANDROID
        var channel = new AndroidNotificationChannel {
            Id = "friends",
            Name = "Friends Channel",
            Importance = Importance.High,
            Description = "Friends notifications",
            EnableLights = true,
            EnableVibration = true,
            LockScreenVisibility = LockScreenVisibility.Public,
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
    }

    public bool LoginFailedLastTime() {
        return PlayerPrefs.GetInt(GOOGLE_LOGIN_FAILED_FLAG_PREF_KEY, 0) != 0;
    }

    int SendNotification(System.TimeSpan delay, string title, string message, Color32 bgColor, bool sound = true, bool vibrate = true, bool lights = true, string bigIcon = "", string smallIcon = "") {
        int id = new System.Random().Next();
        return SendNotification(id, (int)delay.TotalSeconds * 1000, title, message, bgColor, sound, vibrate, lights, bigIcon, smallIcon);
    }

    int SendNotification(int id, System.TimeSpan delay, string title, string message, Color32 bgColor, bool sound = true, bool vibrate = true, bool lights = true, string bigIcon = "", string smallIcon = "") {
        return SendNotification(id, (int)delay.TotalSeconds * 1000, title, message, bgColor, sound, vibrate, lights, bigIcon, smallIcon);
    }

    int SendNotification(int id, long delayMs, string title, string message, Color32 bgColor, bool sound = true, bool vibrate = true, bool lights = true, string bigIcon = "", string smallIcon = "") {
#if UNITY_ANDROID
        AndroidJavaClass pluginClass = new AndroidJavaClass(NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        if (pluginClass != null) {
            pluginClass.CallStatic("SetNotification", id, delayMs, title, message, message,
                sound ? 1 : 0, vibrate ? 1 : 0, lights ? 1 : 0, bigIcon, smallIcon,
        bgColor.r * 65536 + bgColor.g * 256 + bgColor.b, Application.identifier);
        } else {
            Debug.LogErrorFormat("SendNotification: AndroidJavaClass name {0} not found!", NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        }
#endif
        return id;
    }

    public void RegisterSingleNotification(string title, string body, int afterMs, string largeIcon) {
        SendNotification(
            System.TimeSpan.FromMilliseconds(afterMs),
            title,
            body,
            new Color32(0x7f, 0x7f, 0x7f, 255),
            true,
            true,
            true,
            largeIcon,
            "icon1024_2_gray");
    }

    public void RegisterAllNotifications(string title, string body, string largeIcon, int localHours) {
        ClearAllNotifications();

        SetRepeatingNotificationAtMillis(
            (int)RepeatingLocalNotificationId.AT_0900,
            GetNextHourOfDayInMillis(localHours), // 09:00 (KST), 17:00 (PDT), 08:00 (CST)
            GetDayInterval(),
            title,
            body,
            new Color32(239, 58, 38, 255),
            true,
            true,
            true,
            largeIcon,
            "icon1024_2_gray");

        PlatformInterface.instance.logger.Log("RegisterAllRepeatingNotifications");
    }

    enum RepeatingLocalNotificationId {
        AT_0900 = 1,
        AT_1200 = 2,
        AT_1800 = 3,
        AT_0000_TEST = 4,
    }

    public void ClearAllNotifications() {
        CancelPendingNotification((int)RepeatingLocalNotificationId.AT_0900);
        CancelPendingNotification((int)RepeatingLocalNotificationId.AT_1200);
        CancelPendingNotification((int)RepeatingLocalNotificationId.AT_1800);

        ClearAlreadyNotified((int)RepeatingLocalNotificationId.AT_0900);
        ClearAlreadyNotified((int)RepeatingLocalNotificationId.AT_1200);
        ClearAlreadyNotified((int)RepeatingLocalNotificationId.AT_1800);
    }

    public void OnCloudSaveResult(string result) {
        throw new System.NotImplementedException();
    }

    public void OnCloudLoadResult(string result, byte[] data) {
        throw new System.NotImplementedException();
    }

    void CancelPendingNotification(int id) {
#if UNITY_ANDROID
        AndroidJavaClass pluginClass = new AndroidJavaClass(NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        if (pluginClass != null) {
            pluginClass.CallStatic("CancelPendingNotification", id);
        }
#endif
    }

    void ClearAlreadyNotified(int id) {
#if UNITY_ANDROID
        AndroidJavaClass pluginClass = new AndroidJavaClass(NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        if (pluginClass != null) {
            pluginClass.CallStatic("ClearAlreadyNotified", id);
        }
#endif
    }

    int SetRepeatingNotificationAtMillis(int id, long atMillis, long timeoutMs, string title, string message, Color32 bgColor, bool sound, bool vibrate, bool lights, string bigIcon, string smallIcon) {
#if UNITY_ANDROID
        AndroidJavaClass pluginClass = new AndroidJavaClass(NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        if (pluginClass != null) {
            pluginClass.CallStatic("SetRepeatingNotificationAtMillis", id, atMillis, title, message, message, timeoutMs,
                sound ? 1 : 0, vibrate ? 1 : 0, lights ? 1 : 0, bigIcon, smallIcon,
                bgColor.r * 65536 + bgColor.g * 256 + bgColor.b, Application.identifier);
        }
        return id;
#else
        return 0;
#endif
    }

    long GetNextHourOfDayInMillis(int hourOfDay) {
#if UNITY_ANDROID
        AndroidJavaClass pluginClass = new AndroidJavaClass(NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        if (pluginClass != null) {
            return pluginClass.CallStatic<long>("GetNextHourOfDayInMillis", hourOfDay);
        } else {
            Debug.LogErrorFormat("GetNextHourOfDayInMillis: AndroidJavaClass name {0} not found!", NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        }
#endif
        return 0;
    }

    long GetDayInterval() {
#if UNITY_ANDROID
        AndroidJavaClass pluginClass = new AndroidJavaClass(NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        if (pluginClass != null) {
            return pluginClass.CallStatic<long>("GetDayInterval");
        } else {
            Debug.LogErrorFormat("GetDayInterval: AndroidJavaClass name {0} not found!", NOTIFICATION_MANAGER_FULL_CLASS_NAME);
        }
#endif
        return 0;
    }

    public void RequestUserReview() {
        Application.OpenURL(PlatformInterface.instance.config.GetUserReviewUrl());
    }

    public string GetAccountTypeText() {
        return PlatformInterface.instance.textHelper.GetText("platform_account_google");
    }
}
