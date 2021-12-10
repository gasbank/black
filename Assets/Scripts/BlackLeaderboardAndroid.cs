public class BlackLeaderboardAndroid : IBlackLeaderboard
{
    public void UpdateLeaderboard_LastClearedStageId(long value)
    {
        AchievementReport.ReportScore(value, "XXXXXXXXXXXXXXXXXXX", "Leaderboard LastClearedStageId");
    }
}