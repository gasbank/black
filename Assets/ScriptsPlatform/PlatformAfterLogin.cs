#if !NO_GPGS
using GooglePlayGames;
using GooglePlayGames.BasicApi.SavedGame;
#endif
using JetBrains.Annotations;
using UnityEngine;

[DisallowMultipleComponent]
public class PlatformAfterLogin : MonoBehaviour
{
    [SerializeField]
    PlatformInterface platformInterface;

    [SerializeField]
    PlatformSaveUtil platformSaveUtil;

    // 원래 PlatformAndroid.cs에 있어야 하지만...
    // 여기에 있네...ㅋㅋ
    public void ShowSavedGameSelectUI()
    {
        if (Application.isEditor || Application.platform != RuntimePlatform.Android)
        {
            PlatformInterface.Instance.shortMessage.Show("Saved Game Select UI not supported!", true);
            return;
        }
#if !NO_GPGS
        uint maxNumToDisplay = 5;
        const bool allowCreateNew = false; // 새로운 세이브 파일 만드는 것 지원하지 않는다.
        const bool allowDelete = true; // 삭제는 지원하자.

        if (PlayGamesPlatform.Instance != null)
        {
            var savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            if (savedGameClient != null)
                savedGameClient.ShowSelectSavedGameUI(
                    "Select saved game",
                    maxNumToDisplay,
                    allowCreateNew,
                    allowDelete,
                    OnSavedGameSelected);
            else
                Debug.LogWarning("PlayGamesPlatform.Instance.SavedGame null");
        }
        else
        {
            Debug.LogWarning("PlayGamesPlatform.Instance null");
        }
#endif
    }

#if !NO_GPGS
    void OnSavedGameSelected(SelectUIStatus status, ISavedGameMetadata game)
    {
        if (status == SelectUIStatus.SavedGameSelected)
        {
            // handle selected game save
            PlatformInterface.Instance.logger.LogFormat("Save game selection selected! Selected save filename: {0}",
                game.Filename);
            PlatformInterface.Instance.shortMessage.Show("ERROR: Not supported", true);
        }
        else
        {
            // handle cancel or error
            PlatformInterface.Instance.logger.LogFormat("Save game selection canceled! - {0}", status);
        }
    }
#endif
    
    // 우측 버튼 콜백
    [UsedImplicitly]
    public void ShowLeaderboard()
    {
        if (Platform.Instance.CheckLoadSavePrecondition(
                PlatformInterface.Instance.textHelper.GetText("platform_logging_in"),
                () => platformSaveUtil.StartLoginAndDoSomething(() =>
                {
                    PlatformInterface.Instance.confirmPopup.Close();
                    ExecuteShowLeaderboard();
                }), ShowLoginFailed) == false)
            return;

        ExecuteShowLeaderboard();
        PlatformInterface.Instance.progressMessage.Close();
    }

    void ExecuteShowLeaderboard()
    {
        PlatformInterface.Instance.logManager.Add(PlatformInterface.Instance.logEntryType.GameOpenLeaderboard, 0, 0);
        if (Application.isEditor)
            PlatformInterface.Instance.shortMessage.Show("Leaderboard not supported in Editor", true);
        else
            Social.ShowLeaderboardUI();
    }

    void ShowLoginFailed()
    {
        PlatformInterface.Instance.confirmPopup.Open(
            PlatformInterface.Instance.textHelper.GetText("platform_login_failed_popup"));
    }

    // 우측 버튼 콜백
    public void ShowAchievements()
    {
        if (Platform.Instance.CheckLoadSavePrecondition(
                PlatformInterface.Instance.textHelper.GetText("platform_logging_in"),
                () => platformSaveUtil.StartLoginAndDoSomething(() =>
                {
                    PlatformInterface.Instance.confirmPopup.Close();
                    ExecuteShowAchievements();
                }), ShowLoginFailed) == false)
            return;

        ExecuteShowAchievements();
        PlatformInterface.Instance.progressMessage.Close();
    }

    void ExecuteShowAchievements()
    {
        PlatformInterface.Instance.logManager.Add(PlatformInterface.Instance.logEntryType.GameOpenAchievements, 0, 0);
        if (Application.isEditor)
            PlatformInterface.Instance.shortMessage.Show("Achievements not supported in Editor", true);
        else
            Social.ShowAchievementsUI();
    }
}