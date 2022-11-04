using System;
using UnityEngine;

public class Platform : MonoBehaviour
{
    static IPlatformBase platform;

    static readonly string DISABLE_LOGIN_ON_START_KEY = "DISABLE_LOGIN_ON_START";

    [SerializeField]
    PlatformAndroid platformAndroid;

    [SerializeField]
    PlatformEditor platformEditor;

    [SerializeField]
    PlatformInterface platformInterface;

    [SerializeField]
    PlatformIos platformIos;

    [SerializeField]
    PlatformSaveUtil platformSaveUtil;

    public static IPlatformBase Instance => platform;

    public bool DisableLoginOnStart
    {
        get => PlayerPrefs.GetInt(DISABLE_LOGIN_ON_START_KEY, 0) == 1;
        set
        {
            PlatformInterface.Instance.logger.Log($"Platform.DisableLoginOnStart set to {value}");
            PlayerPrefs.SetInt(DISABLE_LOGIN_ON_START_KEY, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    void Awake()
    {
        if (Application.isEditor)
        {
            platform = platformEditor;
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            platform = platformAndroid;
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            platform = platformIos;
        }
        else
        {
            Debug.LogErrorFormat("Platform: Unknown/not supported platform detected: {0}",
                Application.platform);
            // Fallback to editor
            platform = platformEditor;
        }
    }

    internal string GetText(string v)
    {
        return string.Format(PlatformInterface.Instance.textHelper.GetText(v), Instance.GetAccountTypeText());
    }

    public void StartAuthAsync(Action<bool, string> onAuthResult)
    {
        Instance.PreAuthenticate();
        Instance.Login((result, reason) =>
        {
            // StartAuthAsync() 호출한 쪽에서 원하는 처리를 먼저 해 준다.
            onAuthResult(result, reason);

            // 그리고 로그인을 실패했거나 플레이어가 의도적으로 하지 않은 경우를 기록한다.
            // 이 경우라면 다음 게임 실행 시에는 자동 로그인 시작하지 않는다.
            // (수동으로 원할 때까지는)
            DisableLoginOnStart = result == false;
        });
    }

    public void CloudLoad()
    {
        PlatformInterface.Instance.logManager.Add(PlatformInterface.Instance.logEntryType.GameCloudLoadBegin, 0, 0);
        if (Instance.CheckLoadSavePrecondition(PlatformInterface.Instance.textHelper.GetText("platform_loading"),
                platformSaveUtil.StartLoginAndLoad, platformSaveUtil.CancelStartLoginForLoad) == false)
            return;

        Instance.GetCloudLastSavedMetadataAsync(bytes =>
        {
            PlatformInterface.Instance.progressMessage.Close();

            PlatformInterface.Instance.saveUtil.DebugPrintCloudMetadata(bytes);

            if (PlatformInterface.Instance.saveUtil.IsValidCloudMetadata(bytes))
            {
                var overwriteConfirmMsg =
                    PlatformInterface.Instance.saveLoadManager.GetLoadOverwriteConfirmMessage(bytes);

                PlatformInterface.Instance.confirmPopup.OpenYesNoPopup(overwriteConfirmMsg, () =>
                {
                    // 로드하려는 데이터가 현재 플레이하는 것보다 진행이 "덜" 된 것인가?
                    // 경고 한번 더 보여줘야 한다.
                    var rollback = PlatformInterface.Instance.saveLoadManager.IsLoadRollback(bytes);

                    if (rollback)
                    {
                        var msgAgain =
                            PlatformInterface.Instance.textHelper.GetText(
                                "platform_load_confirm_popup_rollback_alert") +
                            "\n\n" + overwriteConfirmMsg;
                        PlatformInterface.Instance.confirmPopup.OpenYesNoPopup(msgAgain, Instance.ExecuteCloudLoad,
                            platformSaveUtil.CancelStartLoginForLoad);
                    }
                    else
                    {
                        Instance.ExecuteCloudLoad();
                    }
                }, platformSaveUtil.CancelStartLoginForLoad);
            }
            else
            {
                platformSaveUtil.NoDataToLoad();
            }
        });
    }

    public void CloudSave()
    {
        PlatformInterface.Instance.logManager.Add(PlatformInterface.Instance.logEntryType.GameCloudSaveBegin, 0, 0);
        if (Instance.CheckLoadSavePrecondition(PlatformInterface.Instance.textHelper.GetText("platform_saving"),
                platformSaveUtil.StartLoginAndSave, platformSaveUtil.CancelStartLoginForSave) == false)
            return;

        Instance.GetCloudLastSavedMetadataAsync(bytes =>
        {
            PlatformInterface.Instance.progressMessage.Close();

            PlatformInterface.Instance.saveUtil.DebugPrintCloudMetadata(bytes);

            if (PlatformInterface.Instance.saveUtil.IsValidCloudMetadata(bytes))
            {
                var overwriteConfirmMsg =
                    PlatformInterface.Instance.saveLoadManager.GetSaveOverwriteConfirmMessage(bytes);

                PlatformInterface.Instance.confirmPopup.OpenYesNoPopup(overwriteConfirmMsg, () =>
                {
                    // 현재 플레이 상황이 덮어쓰려는 저장 데이터보다 진행이 "덜" 된 것인가?
                    // 경고 한번 더 보여줘야 한다.
                    var rollback = PlatformInterface.Instance.saveLoadManager.IsSaveRollback(bytes);

                    if (rollback)
                    {
                        var msgAgain =
                            PlatformInterface.Instance.textHelper.GetText(
                                "platform_save_confirm_popup_rollback_alert") +
                            "\n\n" + overwriteConfirmMsg;
                        PlatformInterface.Instance.confirmPopup.OpenYesNoPopup(msgAgain, Instance.ExecuteCloudSave,
                            platformSaveUtil.CancelStartLoginForSave);
                    }
                    else
                    {
                        Instance.ExecuteCloudSave();
                    }
                }, platformSaveUtil.CancelStartLoginForSave);
            }
            else
            {
                Instance.ExecuteCloudSave();
            }
        });
    }
}