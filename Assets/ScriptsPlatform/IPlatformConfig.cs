public interface IPlatformConfig
{
    string[] FacebookAdsTestDeviceIdList { get; }
    string UnityAdsGameId { get; }
    bool UnityAdsUseTestMode { get; }
    bool UnityAdsUseAds { get; }
    string GetAdMobAppId();
    string GetAdMobRewardVideoAdUnitId();
    string GetFacebookAdsPlacementId();
    string GetUserReviewUrl();
}