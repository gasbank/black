public class BlackLeaderboardIos : IBlackLeaderboard
{
    public void UpdateLeaderboard_LastClearedStageId(long value)
    {
        AchievementReport.ReportScore(value, "top.plusalpha.black", "Leaderboard LastClearedStageId");
    }
}