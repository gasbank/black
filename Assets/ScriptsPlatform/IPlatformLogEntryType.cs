public interface IPlatformLogEntryType
{
    int GameCloudLoadBegin { get; }
    int GameCloudSaveBegin { get; }
    int GameCloudSaveEnd { get; }
    int GameCloudLoadFailure { get; }
    int GameCloudLoadEnd { get; }
    int GameCloudSaveFailure { get; }
    int GameOpenLeaderboard { get; }
    int GameOpenAchievements { get; }
}