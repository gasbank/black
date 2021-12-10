public class BlackLeaderboardEditor : IBlackLeaderboard
{
    public void UpdateLeaderboard_LastClearedStageId(long value)
    {
        AchievementReport.ReportScore(value, "LastClearedStageId", "Leaderboard LastClearedStageId");
    }
}