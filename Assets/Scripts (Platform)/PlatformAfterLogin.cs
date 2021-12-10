using GooglePlayGames;
using GooglePlayGames.BasicApi.SavedGame;
using JetBrains.Annotations;
using UnityEngine;

[DisallowMultipleComponent]
public class PlatformAfterLogin : MonoBehaviour {
    [SerializeField]
    PlatformInterface platformInterface;

    [SerializeField]
    PlatformSaveUtil platformSaveUtil;
    
    // 원래 PlatformAndroid.cs에 있어야 하지만...
    // 여기에 있네...ㅋㅋ
    public void ShowSavedGameSelectUI() {
        if (Application.isEditor || Application.platform != RuntimePlatform.Android) {
            PlatformInterface.instance.shortMessage.Show("Saved Game Select UI not supported!", true);
            return;
        }
#if !NO_GPGS
        uint maxNumToDisplay = 5;
        const bool allowCreateNew = false; // 새로운 세이브 파일 만드는 것 지원하지 않는다.
        const bool allowDelete = true; // 삭제는 지원하자.

        if (PlayGamesPlatform.Instance != null) {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            if (savedGameClient != null) {
                savedGameClient.ShowSelectSavedGameUI(
                    "Select saved game",
                    maxNumToDisplay,
                    allowCreateNew,
                    allowDelete,
                    OnSavedGameSelected);
            } else {
                Debug.LogWarning("PlayGamesPlatform.Instance.SavedGame null");
            }
        } else {
            Debug.LogWarning("PlayGamesPlatform.Instance null");
        }
#endif
    }

    void OnSavedGameSelected(SelectUIStatus status, ISavedGameMetadata game) {
        if (status == SelectUIStatus.SavedGameSelected) {
            // handle selected game save
            PlatformInterface.instance.logger.LogFormat("Save game selection selected! Selected save filename: {0}", game.Filename);
            PlatformInterface.instance.shortMessage.Show("ERROR: Not supported", true);
        } else {
            // handle cancel or error
            PlatformInterface.instance.logger.LogFormat("Save game selection canceled! - {0}", status);
        }
    }

    // 우측 버튼 콜백
    [UsedImplicitly]
    public void ShowLeaderboard() {
        if (Platform.instance.CheckLoadSavePrecondition(PlatformInterface.instance.textHelper.GetText("platform_logging_in"),
                () => platformSaveUtil.StartLoginAndDoSomething(() => {
                    PlatformInterface.instance.confirmPopup.Close();
                    ExecuteShowLeaderboard();
                }), ShowLoginFailed) == false) {
            return;
        }

        ExecuteShowLeaderboard();
        PlatformInterface.instance.progressMessage.Close();
    }

    void ExecuteShowLeaderboard() {
        PlatformInterface.instance.logManager.Add(PlatformInterface.instance.logEntryType.GameOpenLeaderboard, 0, 0);
        if (Application.isEditor) {
            PlatformInterface.instance.shortMessage.Show("Leaderboard not supported in Editor", true);
        } else {
            Social.ShowLeaderboardUI();
        }
    }

    void ShowLoginFailed() {
        PlatformInterface.instance.confirmPopup.Open(PlatformInterface.instance.textHelper.GetText("platform_login_failed_popup"));
    }

    // 우측 버튼 콜백
    public void ShowAchievements() {
        if (Platform.instance.CheckLoadSavePrecondition(PlatformInterface.instance.textHelper.GetText("platform_logging_in"),
                () => platformSaveUtil.StartLoginAndDoSomething(() => {
                    PlatformInterface.instance.confirmPopup.Close();
                    ExecuteShowAchievements();
                }), ShowLoginFailed) == false) {
            return;
        }

        ExecuteShowAchievements();
        PlatformInterface.instance.progressMessage.Close();
    }

    void ExecuteShowAchievements() {
        PlatformInterface.instance.logManager.Add(PlatformInterface.instance.logEntryType.GameOpenAchievements, 0, 0);
        if (Application.isEditor) {
            PlatformInterface.instance.shortMessage.Show("Achievements not supported in Editor", true);
        } else {
            Social.ShowAchievementsUI();
        }
    }
}