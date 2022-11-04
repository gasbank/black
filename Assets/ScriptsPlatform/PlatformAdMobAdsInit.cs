#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif
#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
#endif

using UnityEngine;

[DisallowMultipleComponent]
public class PlatformAdMobAdsInit : MonoBehaviour
{
    public static PlatformAdMobAdsInit Instance;

    [SerializeField]
    PlatformAdMobAds platformAdMobAds;

#if GOOGLE_MOBILE_ADS
    bool shouldBeRewarded;
    public RewardedAd RewardBasedVideo;

    public void Init()
    {
        PlatformInterface.Instance.logger.Log("PlatformAdMobAdsInit.Start()");

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(status => { });

        var testDeviceList = new List<string>
        {
            //"751709a03251817c6a3d7d3f7072ec57" // iPhone 6s
        };
        var requestConfiguration = new RequestConfiguration.Builder().SetTestDeviceIds(testDeviceList).build();
        MobileAds.SetRequestConfiguration(requestConfiguration);

        // 광고 사운드 끄기
        MobileAds.SetApplicationMuted(true);

        var adUnitId = PlatformInterface.Instance.config.GetAdMobRewardVideoAdUnitId();

        // Get singleton reward based video ad reference.
        RewardBasedVideo = new(adUnitId);

        RewardBasedVideo.OnAdLoaded += HandleOnAdLoaded;
        RewardBasedVideo.OnAdFailedToLoad += HandleAdFailedToLoad;
        RewardBasedVideo.OnAdOpening += HandleAdOpening;
        RewardBasedVideo.OnAdClosed += HandleAdClosed;
        RewardBasedVideo.OnAdFailedToShow += HandleAdFailedToShow;
        RewardBasedVideo.OnAdDidRecordImpression += HandleAdDidRecordImpression;
        RewardBasedVideo.OnUserEarnedReward += HandleUserEarnedReward;
        RewardBasedVideo.OnPaidEvent += HandlePaidEvent;

        RequestRewardBasedVideo();

#if UNITY_IOS
        var trackingStatus = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
        Debug.Log($"ATTrackingStatusBinding.GetAuthorizationTrackingStatus()={trackingStatus}");
        switch (trackingStatus)
        {
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED:
                ATTrackingStatusBinding.RequestAuthorizationTracking();
                break;
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED:
#if BLACK_FACEBOOK
                AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(true);
#endif
                break;
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.RESTRICTED:
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED:
#if BLACK_FACEBOOK
                AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(false);
#endif
                break;
        }
#endif
    }

    void HandlePaidEvent(object sender, AdValueEventArgs e)
    {
        PlatformInterface.Instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandlePaidEvent)}");
    }

    void HandleAdDidRecordImpression(object sender, EventArgs e)
    {
        PlatformInterface.Instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleAdDidRecordImpression)}");
    }

    void HandleOnAdLoaded(object sender, EventArgs args)
    {
        PlatformInterface.Instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleOnAdLoaded)}");
    }

    void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        var errorMessage = args.LoadAdError.GetMessage();
        PlatformInterface.Instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleAdFailedToLoad)}");
        PlatformInterface.Instance.logger.Log($"    Message: {errorMessage}");
        UnityMainThreadDispatcher.Instance().Enqueue(() => PlatformAdMobAds.HandleFailedToLoad(errorMessage));
    }

    void HandleAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        var errorMessage = args.AdError.GetMessage();
        PlatformInterface.Instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleAdFailedToShow)}");
        PlatformInterface.Instance.logger.Log($"    Message: {errorMessage}");
        UnityMainThreadDispatcher.Instance().Enqueue(() => PlatformAdMobAds.HandleFailedToLoad(errorMessage));
    }

    void HandleAdOpening(object sender, EventArgs args)
    {
        PlatformInterface.Instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleAdOpening)}");
        shouldBeRewarded = false;
    }

    void HandleAdClosed(object sender, EventArgs args)
    {
        PlatformInterface.Instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleAdClosed)}");
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            RequestRewardBasedVideo();
            if (shouldBeRewarded)
            {
                platformAdMobAds.HandleRewarded();
            }
        });
    }

    void HandleUserEarnedReward(object sender, Reward args)
    {
        PlatformInterface.Instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleUserEarnedReward)}");
        string type = args.Type;
        double amount = args.Amount;
        PlatformInterface.Instance.logger.Log($"HandleRewardBasedVideoRewarded event received for {amount} {type}");
        shouldBeRewarded = true;
    }

    void RequestRewardBasedVideo()
    {
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded video ad with the request.
        RewardBasedVideo.LoadAd(request);
    }

    public static void TestMediation()
    {
        // Google Mobile Ads 업데이트되면서 더이상 지원되지 않는건가?
    }
#endif
}