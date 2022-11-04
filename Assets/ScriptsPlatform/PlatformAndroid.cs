using UnityEngine;
using System;
using Random = System.Random;
#if !NO_GPGS
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
#endif

public class PlatformAndroid : MonoBehaviour, IPlatformBase
{
    static readonly string GOOGLE_LOGIN_FAILED_FLAG_PREF_KEY = "__google_login_failed_flag";
    public static bool OnSavedGameOpenedAndWriteAlwaysInternalError = false;

    [SerializeField]
    Platform platform;

    [SerializeField]
    PlatformSaveUtil platformSaveUtil;

    public void Logout()
    {
        throw new NotImplementedException();
    }

    public bool CheckLoadSavePrecondition(string progressMessage, Action onNotLoggedIn, Action onAbort)
    {
        if (!PlatformLogin.IsAuthenticated)
        {
            PlatformInterface.Instance.confirmPopup.OpenYesNoPopup(
                platform.GetText("platform_google_login_required_popup"), onNotLoggedIn, onAbort);
            return false;
        }

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
#if !NO_GPGS
        if (!PlatformLogin.IsAuthenticated)
        {
            PlatformInterface.Instance.logger.LogFormat("GetCloudSavedAccountData: not authenticated");
            onPeekResult(null);
            return;
        }

        var savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        if (savedGameClient != null)
        {
            Open(savedGameClient,
                true,
                OnSavedGameOpenedAndReadConflictResolve,
                (status, game) =>
                {
                    if (status == SavedGameRequestStatus.Success)
                    {
                        // handle reading or writing of saved game.

                        PlatformInterface.Instance.logger.LogFormat(
                            "GetCloudSavedAccountData: Save game open (read) success! Filename: {0}", game.Filename);

                        savedGameClient.ReadBinaryData(game, (status2, data2) =>
                        {
                            if (status == SavedGameRequestStatus.Success)
                            {
                                // handle processing the byte array data
                                PlatformInterface.Instance.logger.LogFormat(
                                    "GetCloudSavedAccountData success! - Data size: {0} bytes", data2.Length);
                                try
                                {
                                    onPeekResult(data2);
                                }
                                catch
                                {
                                    PlatformInterface.Instance.logger.LogFormat(
                                        "GetCloudSavedAccountData: Exception at deserialization");
                                    onPeekResult(null);
                                }
                            }
                            else
                            {
                                PlatformInterface.Instance.logger.LogFormat(
                                    "GetCloudSavedAccountData: ReadBinaryData error! - {0}", status2);
                                onPeekResult(null);
                            }
                        });
                    }
                    else
                    {
                        platformSaveUtil.LogCloudLoadSaveError(
                            $"GetCloudSavedAccountData: OpenWithAutomaticConflictResolution error! - {status}");
                        onPeekResult(null);
                    }
                });
        }
        else
        {
            platformSaveUtil.LogCloudLoadSaveError("GetCloudSavedAccountData: savedGameClient null");
            onPeekResult(null);
        }
#endif
    }

    public void ExecuteCloudLoad()
    {
#if !NO_GPGS
        platformSaveUtil.ShowLoadProgressPopup();

        var savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        if (savedGameClient != null)
        {
            Open(savedGameClient,
                true,
                OnSavedGameOpenedAndReadConflictResolve,
                OnSavedGameOpenedAndRead);
        }
        else
        {
            // handle error
            platformSaveUtil.ShowLoadErrorPopup("OnClick_cloudSave: savedGameClient null");
            PlatformInterface.Instance.logManager.Add(PlatformInterface.Instance.logEntryType.GameCloudLoadFailure, 0,
                2);
        }
#endif
    }

    public void ExecuteCloudSave()
    {
#if !NO_GPGS
        PlatformInterface.Instance.saveLoadManager.SaveBeforeCloudSave();
        platformSaveUtil.ShowSaveProgressPopup();

        var savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        if (savedGameClient != null)
        {
            Open(savedGameClient,
                true,
                OnSavedGameOpenedAndWriteConflictResolve,
                OnSavedGameOpenedAndWrite);
        }
        else
        {
            platformSaveUtil.ShowSaveErrorPopup("OnClick_cloudSave: savedGameClient null");
            PlatformInterface.Instance.logManager.Add(PlatformInterface.Instance.logEntryType.GameCloudSaveFailure, 0,
                1);
        }
#endif
    }

    public void Login(Action<bool, string> onAuthResult)
    {
        Social.localUser.Authenticate((b, reason) =>
        {
            // 구글 로그인 성공/실패 유무에 따른 플래그 업데이트
            PlayerPrefs.SetInt(GOOGLE_LOGIN_FAILED_FLAG_PREF_KEY, b ? 0 : 1);
            PlayerPrefs.Save();
            onAuthResult(b, reason);
        });
    }

    public void PreAuthenticate()
    {
#if !NO_GPGS
        //PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
#endif

#if UNITY_ANDROID && BLACK_NOTIFICATION
        var channel = new AndroidNotificationChannel
        {
            Id = "friends",
            Name = "Friends Channel",
            Importance = Importance.High,
            Description = "Friends notifications",
            EnableLights = true,
            EnableVibration = true,
            LockScreenVisibility = LockScreenVisibility.Public
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
    }

    public bool LoginFailedLastTime()
    {
        return PlayerPrefs.GetInt(GOOGLE_LOGIN_FAILED_FLAG_PREF_KEY, 0) != 0;
    }

    public void RegisterSingleNotification(string title, string body, int afterMs, string largeIcon)
    {
        SendNotification(
            TimeSpan.FromMilliseconds(afterMs),
            title,
            body,
            new(0x7f, 0x7f, 0x7f, 255),
            true,
            true,
            true,
            largeIcon,
            "icon1024_2_gray");
    }

    public void RegisterAllNotifications(string title, string body, string largeIcon, int localHours)
    {
        ClearAllNotifications();

        RegisterDailyChannel();

#if UNITY_ANDROID && BLACK_NOTIFICATION
        var notification = new AndroidNotification
        {
            Title = title,
            Text = body,
            FireTime = PlatformEditor.GetNextLocalHours(localHours),
            RepeatInterval = TimeSpan.FromDays(1)
        };
        AndroidNotificationCenter.SendNotification(notification, "DailyChannel");
#endif

        PlatformInterface.Instance.logger.Log("RegisterAllRepeatingNotifications");
    }

    public void ClearAllNotifications()
    {
#if UNITY_ANDROID && BLACK_NOTIFICATION
        AndroidNotificationCenter.CancelAllNotifications();
#endif
    }

    public void OnCloudSaveResult(string result)
    {
        throw new NotImplementedException();
    }

    public void OnCloudLoadResult(string result, byte[] data)
    {
        throw new NotImplementedException();
    }

    public void RequestUserReview()
    {
        Application.OpenURL(PlatformInterface.Instance.config.GetUserReviewUrl());
    }

    public string GetAccountTypeText()
    {
        return PlatformInterface.Instance.textHelper.GetText("platform_account_google");
    }

#if !NO_GPGS
    void Open(ISavedGameClient savedGameClient, bool useAutomaticResolution, ConflictCallback conflictCallback,
        Action<SavedGameRequestStatus, ISavedGameMetadata> completedCallback)
    {
        if (useAutomaticResolution)
            savedGameClient.OpenWithAutomaticConflictResolution(
                PlatformSaveUtil.remoteSaveFileName,
                DataSource.ReadNetworkOnly,
                ConflictResolutionStrategy.UseLongestPlaytime,
                completedCallback);
        else
            savedGameClient.OpenWithManualConflictResolution(
                PlatformSaveUtil.remoteSaveFileName,
                DataSource.ReadNetworkOnly,
                true,
                conflictCallback,
                completedCallback);
    }
#endif

    static void SendNotification(TimeSpan delay, string title, string message, Color32 bgColor, bool sound = true,
        bool vibrate = true, bool lights = true, string bigIcon = "", string smallIcon = "")
    {
        var id = new Random().Next();
        SendNotification((int) delay.TotalSeconds * 1000, title, message, bgColor, sound, vibrate, lights,
            bigIcon, smallIcon);
    }

    static void SendNotification(long delayMs, string title, string message, Color32 bgColor, bool sound = true,
        bool vibrate = true, bool lights = true, string bigIcon = "", string smallIcon = "")
    {
        RegisterDailyChannel();
#if UNITY_ANDROID && BLACK_NOTIFICATION
        var notification = new AndroidNotification
        {
            Title = title,
            Text = message,
            FireTime = DateTime.Now.AddDays(1)
        };
        AndroidNotificationCenter.SendNotification(notification, "daily_channel_id");
#endif
    }

    static void RegisterDailyChannel()
    {
#if UNITY_ANDROID && BLACK_NOTIFICATION
        var channel = new AndroidNotificationChannel
        {
            Id = "daily_channel_id",
            Name = "Daily Channel",
            Importance = Importance.Default,
            Description = "Color Museum Daily Events"
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
    }

    long GetNextHourOfDayInMillis(int hourOfDay)
    {
        return 0;
    }

    long GetDayInterval()
    {
        return 0;
    }

#if !NO_GPGS
    void OnSavedGameOpenedAndWriteConflictResolve(IConflictResolver resolver, ISavedGameMetadata original,
        byte[] originalData, ISavedGameMetadata unmerged, byte[] unmergedData)
    {
        resolver.ChooseMetadata(unmerged);
    }

    void OnSavedGameOpenedAndReadConflictResolve(IConflictResolver resolver, ISavedGameMetadata original,
        byte[] originalData, ISavedGameMetadata unmerged, byte[] unmergedData)
    {
        resolver.ChooseMetadata(original);
    }

    public void OnSavedGameOpenedAndWrite(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        // 코너 케이스 테스트를 위한 코드
        if (OnSavedGameOpenedAndWriteAlwaysInternalError) status = SavedGameRequestStatus.InternalError;

        if (status == SavedGameRequestStatus.Success)
        {
            // handle reading or writing of saved game.

            PlatformInterface.Instance.logger.LogFormat(
                "OnSavedGameOpenedAndWrite: Save game open (write) success! Filename: {0}", game.Filename);

            SerializeAndSaveGame(game);
        }
        else
        {
            // handle error
            if (status == SavedGameRequestStatus.InternalError)
                // Google Play 게임 앱 버전이 낮아서 InternalError가 나는 사례도 두 번 정도 제보되었다.
                // 여기서 관련된 정보를 추가로 알려주면 어쩌면 좋을지도...?
                platformSaveUtil.ShowSaveErrorPopupWithGooglePlayGamesUpdateButton(
                    $"OnSavedGameOpenedAndWrite: Save game open (write) failed! - {status}\nPlease consider updating Google Play Games app.");
            else
                platformSaveUtil.ShowSaveErrorPopup(
                    $"OnSavedGameOpenedAndWrite: Save game open (write) failed! - {status}");

            //rootCanvasGroup.interactable = true;
            PlatformInterface.Instance.logManager.Add(PlatformInterface.Instance.logEntryType.GameCloudSaveFailure, 0,
                2);
        }
    }

    void SerializeAndSaveGame(ISavedGameMetadata game)
    {
        var savedData = PlatformInterface.Instance.saveUtil.SerializeSaveData();
        var played = PlatformInterface.Instance.saveUtil.GetPlayed();
        var desc = PlatformInterface.Instance.saveUtil.GetDesc(savedData);
        SaveGame(game, savedData, played, desc);
    }
#endif

#if !NO_GPGS
    void SaveGame(ISavedGameMetadata game, byte[] savedData, TimeSpan totalPlaytime, string desc)
    {
        var savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        var builder = new SavedGameMetadataUpdate.Builder();
        builder = builder.WithUpdatedDescription(desc);
        builder = builder.WithUpdatedPlayedTime(totalPlaytime);
        var updatedMetadata = builder.Build();
        savedGameClient.CommitUpdate(game, updatedMetadata, savedData, OnSavedGameWritten);
    }

    void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            // handle reading or writing of saved game.
            platformSaveUtil.ShowSaveResultPopup();
        }
        else
        {
            // handle error
            platformSaveUtil.ShowSaveErrorPopup($"OnSavedGameWritten: OnSavedGameWritten failed! - {status}");
            PlatformInterface.Instance.logManager.Add(PlatformInterface.Instance.logEntryType.GameCloudSaveFailure, 0,
                3);
        }

        //rootCanvasGroup.interactable = true;
    }

    public void OnSavedGameOpenedAndRead(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            // handle reading or writing of saved game.

            PlatformInterface.Instance.logger.LogFormat("Save game open (read) success! Filename: {0}", game.Filename);

            LoadGameData(game);
        }
        else
        {
            // handle error
            platformSaveUtil.ShowLoadErrorPopup("OnSavedGameOpenedAndRead: status != SavedGameRequestStatus.Success");
            PlatformInterface.Instance.logManager.Add(PlatformInterface.Instance.logEntryType.GameCloudLoadFailure, 0,
                3);
        }
    }
#endif

#if !NO_GPGS
    void LoadGameData(ISavedGameMetadata game)
    {
        var savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.ReadBinaryData(game, OnSavedGameDataRead);
    }

    void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] data)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            // handle processing the byte array data
            PlatformInterface.Instance.logger.LogFormat("OnSavedGameDataRead success! - Data size: {0} bytes",
                data.Length);

            var remoteSaveDict = PlatformInterface.Instance.saveUtil.DeserializeSaveData(data);

            PlatformInterface.Instance.saveUtil.LoadDataAndLoadSplashScene(remoteSaveDict);
        }
        else
        {
            // handle error
            platformSaveUtil.ShowLoadErrorPopup("OnSavedGameDataRead: status == SavedGameRequestStatus.Success");
            PlatformInterface.Instance.logManager.Add(PlatformInterface.Instance.logEntryType.GameCloudLoadFailure, 0,
                4);
        }
    }
#endif
}