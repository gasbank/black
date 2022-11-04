public class AchievementReport
{
    public static void ReportScore(long value, string code, string text)
    {
        SocialScoreReporter.Instance.QueueScore(code, value, text);
    }
}