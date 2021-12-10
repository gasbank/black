public interface IPlatformConfig {
    string GetAdMobAppId();
    string GetAdMobRewardVideoAdUnitId();
    string NotificationManagerFullClassName { get; }
    string ScreenshotAndReportFullClassName { get; }
    string GetFacebookAdsPlacementId();
    string[] FacebookAdsTestDeviceIdList { get; }
    string UnityAdsGameId { get; }
    bool UnityAdsUseTestMode { get; }
    bool UnityAdsUseAds { get; }
    string GetUserReviewUrl();
}