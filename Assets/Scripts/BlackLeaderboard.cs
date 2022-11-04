using UnityEngine;

public static class BlackLeaderboard
{
    static IBlackLeaderboard blackLeaderboard;

    public static IBlackLeaderboard Instance
    {
        get
        {
            if (blackLeaderboard != null) return blackLeaderboard;

            if (Application.isEditor)
            {
                blackLeaderboard = new BlackLeaderboardEditor();
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                blackLeaderboard = new BlackLeaderboardAndroid();
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                blackLeaderboard = new BlackLeaderboardIos();
            }
            else
            {
                Debug.LogErrorFormat("BlackLeaderboard: Unknown/not supported platform detected: {0}",
                    Application.platform);
                // Fallback to editor
                blackLeaderboard = new BlackLeaderboardEditor();
            }

            return blackLeaderboard;
        }
    }
}