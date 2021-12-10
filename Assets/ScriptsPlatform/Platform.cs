using UnityEngine;

public class Platform : MonoBehaviour {
    static IPlatformBase platform;

    [SerializeField]
    PlatformEditor platformEditor;

    [SerializeField]
    PlatformAndroid platformAndroid;

    [SerializeField]
    PlatformIos platformIos;

    [SerializeField]
    PlatformInterface platformInterface;

    [SerializeField]
    PlatformSaveUtil platformSaveUtil;

    void Awake() {
        if (Application.isEditor) {
            platform = platformEditor;
        } else if (Application.platform == RuntimePlatform.Android) {
            platform = platformAndroid;
        } else if (Application.platform == RuntimePlatform.IPhonePlayer) {
            platform = platformIos;
        } else {
            Debug.LogErrorFormat("Platform: Unknown/not supported platform detected: {0}",
                Application.platform);
            // Fallback to editor
            platform = platformEditor;
        }
    }

    public static IPlatformBase instance => platform;

    static readonly string DISABLE_LOGIN_ON_START_KEY = "DISABLE_LOGIN_ON_START";

    public bool DisableLoginOnStart {
        get => PlayerPrefs.GetInt(DISABLE_LOGIN_ON_START_KEY, 0) == 1;
        set {
            PlatformInterface.instance.logger.Log($"Platform.DisableLoginOnStart set to {value}");
            PlayerPrefs.SetInt(DISABLE_LOGIN_ON_START_KEY, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    internal string GetText(string v) {
        return string.Format(PlatformInterface.instance.textHelper.GetText(v), instance.GetAccountTypeText());
    }

    public void StartAuthAsync(System.Action<bool, string> onAuthResult) {
        instance.PreAuthenticate();
        instance.Login((result, reason) => {
            // StartAuthAsync() 호출한 쪽에서 원하는 처리를 먼저 해 준다.
            onAuthResult(result, reason);

            // 그리고 로그인을 실패했거나 플레이어가 의도적으로 하지 않은 경우를 기록한다.
            // 이 경우라면 다음 게임 실행 시에는 자동 로그인 시작하지 않는다.
            // (수동으로 원할 때까지는)
            DisableLoginOnStart = (result == false);
        });
    }

    public void CloudLoad() {
        PlatformInterface.instance.logManager.Add(PlatformInterface.instance.logEntryType.GameCloudLoadBegin, 0, 0);
        if (instance.CheckLoadSavePrecondition(PlatformInterface.instance.textHelper.GetText("platform_loading"),
                platformSaveUtil.StartLoginAndLoad, platformSaveUtil.CancelStartLoginForLoad) == false) {
            return;
        }

        instance.GetCloudLastSavedMetadataAsync((bytes) => {
            PlatformInterface.instance.progressMessage.Close();

            PlatformInterface.instance.saveUtil.DebugPrintCloudMetadata(bytes);

            if (PlatformInterface.instance.saveUtil.IsValidCloudMetadata(bytes)) {
                var overwriteConfirmMsg =
                    PlatformInterface.instance.saveLoadManager.GetLoadOverwriteConfirmMessage(bytes);

                PlatformInterface.instance.confirmPopup.OpenYesNoPopup(overwriteConfirmMsg, () => {
                    // 로드하려는 데이터가 현재 플레이하는 것보다 진행이 "덜" 된 것인가?
                    // 경고 한번 더 보여줘야 한다.
                    var rollback = PlatformInterface.instance.saveLoadManager.IsLoadRollback(bytes);

                    if (rollback) {
                        var msgAgain =
                            PlatformInterface.instance.textHelper.GetText("platform_load_confirm_popup_rollback_alert") +
                            "\n\n" + overwriteConfirmMsg;
                        PlatformInterface.instance.confirmPopup.OpenYesNoPopup(msgAgain, instance.ExecuteCloudLoad,
                            platformSaveUtil.CancelStartLoginForLoad);
                    } else {
                        instance.ExecuteCloudLoad();
                    }
                }, platformSaveUtil.CancelStartLoginForLoad);
            } else {
                platformSaveUtil.NoDataToLoad();
            }
        });
    }

    public void CloudSave() {
        PlatformInterface.instance.logManager.Add(PlatformInterface.instance.logEntryType.GameCloudSaveBegin, 0, 0);
        if (instance.CheckLoadSavePrecondition(PlatformInterface.instance.textHelper.GetText("platform_saving"),
                platformSaveUtil.StartLoginAndSave, platformSaveUtil.CancelStartLoginForSave) == false) {
            return;
        }

        instance.GetCloudLastSavedMetadataAsync((bytes) => {
            PlatformInterface.instance.progressMessage.Close();

            PlatformInterface.instance.saveUtil.DebugPrintCloudMetadata(bytes);

            if (PlatformInterface.instance.saveUtil.IsValidCloudMetadata(bytes)) {
                var overwriteConfirmMsg =
                    PlatformInterface.instance.saveLoadManager.GetSaveOverwriteConfirmMessage(bytes);

                PlatformInterface.instance.confirmPopup.OpenYesNoPopup(overwriteConfirmMsg, () => {
                    // 현재 플레이 상황이 덮어쓰려는 저장 데이터보다 진행이 "덜" 된 것인가?
                    // 경고 한번 더 보여줘야 한다.
                    var rollback = PlatformInterface.instance.saveLoadManager.IsSaveRollback(bytes);

                    if (rollback) {
                        var msgAgain =
                            PlatformInterface.instance.textHelper.GetText("platform_save_confirm_popup_rollback_alert") +
                            "\n\n" + overwriteConfirmMsg;
                        PlatformInterface.instance.confirmPopup.OpenYesNoPopup(msgAgain, instance.ExecuteCloudSave,
                            platformSaveUtil.CancelStartLoginForSave);
                    } else {
                        instance.ExecuteCloudSave();
                    }
                }, platformSaveUtil.CancelStartLoginForSave);
            } else {
                instance.ExecuteCloudSave();
            }
        });
    }
}