#if !NO_GPGS
using GooglePlayGames.BasicApi.SavedGame;
#endif
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PlatformSaveUtil : MonoBehaviour
{
    public static readonly string remoteSaveFileName = "default-remote-save";

    static readonly string firebaseBaseUrl = "https://xxxxxxxxxxx-a41cb.firebaseio.com";

    //private static readonly string couponUrlFormat = firebaseBaseUrl + "/coupons/{0}.json";
    public static readonly string noticeUrlFormat = firebaseBaseUrl + "/notice.json";
    public static readonly string saveUrlFormat = "https://xxxxxxxxxx-35636519.firebaseio.com/saves/{0}.json";

    [SerializeField]
    Platform platform;

    [SerializeField]
    PlatformInterface platformInterface;

    public static string RemoteSaveFileForEditor => Application.persistentDataPath + "/" + remoteSaveFileName;


#if !NO_GPGS
    static void OnSavedGameOpenedAndWriteConflictResolve(IConflictResolver resolver, ISavedGameMetadata original,
        byte[] originalData, ISavedGameMetadata unmerged, byte[] unmergedData)
    {
        resolver.ChooseMetadata(unmerged);
    }

    static void OnSavedGameOpenedAndReadConflictResolve(IConflictResolver resolver, ISavedGameMetadata original,
        byte[] originalData, ISavedGameMetadata unmerged, byte[] unmergedData)
    {
        resolver.ChooseMetadata(original);
    }
#endif
    
    public void LogCloudLoadSaveError(string message)
    {
        Debug.LogError(message);
    }

    public void CancelStartLoginForSave()
    {
        PlatformInterface.Instance.confirmPopup.Open(
            PlatformInterface.Instance.textHelper.GetText("platform_save_cancelled_popup"));
        PlatformInterface.Instance.logManager.Add(PlatformInterface.Instance.logEntryType.GameCloudSaveFailure, 0, 0);
    }

    public void ShowSaveProgressPopup()
    {
        PlatformInterface.Instance.progressMessage.Open(
            PlatformInterface.Instance.textHelper.GetText("platform_saving"));
    }

    public void ShowLoadProgressPopup()
    {
        PlatformInterface.Instance.progressMessage.Open(
            PlatformInterface.Instance.textHelper.GetText("platform_loading"));
    }

    public void ShowSaveErrorPopup(string text)
    {
        LogCloudLoadSaveError(text);

        PlatformInterface.Instance.progressMessage.Close();
        PlatformInterface.Instance.confirmPopup.Open(text);
    }

    public void ShowSaveErrorPopupWithGooglePlayGamesUpdateButton(string text)
    {
        LogCloudLoadSaveError(text);

        PlatformInterface.Instance.progressMessage.Close();
        PlatformInterface.Instance.confirmPopup.OpenTwoButtonPopup_Update(text,
            PlatformInterface.Instance.confirmPopup.Close,
            () => { Application.OpenURL("market://details?id=com.google.android.play.games"); });
    }

    public void CancelStartLoginForLoad()
    {
        PlatformInterface.Instance.confirmPopup.Open(
            PlatformInterface.Instance.textHelper.GetText("platform_load_cancelled_popup"));
        PlatformInterface.Instance.logManager.Add(PlatformInterface.Instance.logEntryType.GameCloudLoadFailure, 0, 0);
    }

    public void NoDataToLoad()
    {
        PlatformInterface.Instance.confirmPopup.Open(
            PlatformInterface.Instance.textHelper.GetText("platform_cloud_load_fail"));
        PlatformInterface.Instance.logManager.Add(PlatformInterface.Instance.logEntryType.GameCloudLoadFailure, 0, 1);
    }

    public void ShowLoadErrorPopup(string text)
    {
        LogCloudLoadSaveError(text);

        PlatformInterface.Instance.progressMessage.Close();
        PlatformInterface.Instance.confirmPopup.Open(text);
    }

    public void ShowPeekProgressPopup()
    {
        PlatformInterface.Instance.progressMessage.Open(
            PlatformInterface.Instance.textHelper.GetText("platform_check_last_saved"));
    }

    public void StartLoginAndDoSomething(Action something)
    {
        // 유저가 직접 로그인 시도한 것이기 때문에 과거의 로그인 실패 여부는 따지지 않는다.
        PlatformInterface.Instance.progressMessage.Open(
            PlatformInterface.Instance.textHelper.GetText("platform_logging_in"));
        platform.StartAuthAsync((b, reason) =>
        {
            PlatformInterface.Instance.progressMessage.Close();
            if (b)
            {
                something();
            }
            else
            {
                Debug.LogErrorFormat("Login failed - reason: {0}", reason);

                PlatformInterface.Instance.confirmPopup.Open(
                    platform.GetText("platform_login_failed_popup") + "\n\n" + reason);
            }
        });
    }

    public void StartLoginAndLoad()
    {
        StartLoginAndDoSomething(platform.CloudLoad);
    }

    public void StartLoginAndSave()
    {
        // 유저가 직접 로그인 시도한 것이기 때문에 과거의 로그인 실패 여부는 따지지 않는다.
        PlatformInterface.Instance.progressMessage.Open(
            PlatformInterface.Instance.textHelper.GetText("platform_logging_in"));
        platform.StartAuthAsync((b, reason) =>
        {
            PlatformInterface.Instance.progressMessage.Close();
            if (b)
            {
                platform.CloudSave();
            }
            else
            {
                Debug.LogErrorFormat("Login failed - reason: {0}", reason);

                PlatformInterface.Instance.confirmPopup.Open(
                    platform.GetText("platform_login_failed_popup") + "\n\n" + reason);
            }
        });
    }

    public void ShowSaveResultPopup()
    {
        PlatformInterface.Instance.progressMessage.Close();
        PlatformInterface.Instance.confirmPopup.Open(
            PlatformInterface.Instance.textHelper.GetText("platform_saved_popup"));
        PlatformInterface.Instance.logManager.Add(PlatformInterface.Instance.logEntryType.GameCloudSaveEnd, 0, 0);
    }

    public IEnumerator ReportCorruptSaveFileSendCoroutine(string guid, string reportJsonStr)
    {
        using (var request = UnityWebRequest.Put(string.Format(saveUrlFormat, guid), reportJsonStr))
        {
            yield return request.SendWebRequest();
        }

        PlatformInterface.Instance.logger.LogFormat("ReportCorruptSaveFile report finished. GUID {0}", guid);
    }
}