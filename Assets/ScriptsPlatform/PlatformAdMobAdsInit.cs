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
    public static PlatformAdMobAdsInit instance;

    [SerializeField]
    PlatformInterface platformInterface;

    [SerializeField]
    PlatformAdMobAds platformAdMobAds;

#if GOOGLE_MOBILE_ADS
    bool shouldBeRewarded;
    public RewardedAd rewardBasedVideo;

    public void Start() {
        PlatformInterface.instance.logger.Log("PlatformAdMobAdsInit.Start()");

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(status => { });
        
        var testDeviceList = new List<string> {
            //"751709a03251817c6a3d7d3f7072ec57" // iPhone 6s
        };
        var requestConfiguration = new RequestConfiguration.Builder().SetTestDeviceIds(testDeviceList).build();
        MobileAds.SetRequestConfiguration(requestConfiguration);

        // 광고 사운드 끄기
        MobileAds.SetApplicationMuted(true);

        var adUnitId = PlatformInterface.instance.config.GetAdMobRewardVideoAdUnitId();

        // Get singleton reward based video ad reference.
        rewardBasedVideo = new RewardedAd(adUnitId);

        rewardBasedVideo.OnAdLoaded += HandleOnAdLoaded;
        rewardBasedVideo.OnAdFailedToLoad += HandleAdFailedToLoad;
        rewardBasedVideo.OnAdOpening += HandleAdOpening;
        rewardBasedVideo.OnAdClosed += HandleAdClosed;
        rewardBasedVideo.OnAdFailedToShow += HandleAdFailedToShow;
        rewardBasedVideo.OnAdDidRecordImpression += HandleAdDidRecordImpression;
        rewardBasedVideo.OnUserEarnedReward += HandleUserEarnedReward;
        rewardBasedVideo.OnPaidEvent += HandlePaidEvent;
        
        RequestRewardBasedVideo();

#if UNITY_IOS && BLACK_FACEBOOK
        var trackingStatus = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
        Debug.Log($"ATTrackingStatusBinding.GetAuthorizationTrackingStatus()={trackingStatus}");
        switch (trackingStatus)
        {
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED:
                ATTrackingStatusBinding.RequestAuthorizationTracking();
                break;
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED:
                AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(true);
                break;
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.RESTRICTED:
            case ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED:
                AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(false);
                break;
        }
#endif
    }

    void HandlePaidEvent(object sender, AdValueEventArgs e) {
        PlatformInterface.instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandlePaidEvent)}");
    }

    void HandleAdDidRecordImpression(object sender, EventArgs e) {
        PlatformInterface.instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleAdDidRecordImpression)}");
    }

    void HandleOnAdLoaded(object sender, EventArgs args) {
        PlatformInterface.instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleOnAdLoaded)}");
    }

    void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs args) {
        var errorMessage = args.LoadAdError.GetMessage();
        PlatformInterface.instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleAdFailedToLoad)}");
        PlatformInterface.instance.logger.Log($"    Message: {errorMessage}");
        UnityMainThreadDispatcher.Instance().Enqueue(() => PlatformAdMobAds.HandleFailedToLoad(errorMessage));
    }
    
    void HandleAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        var errorMessage = args.AdError.GetMessage();
        PlatformInterface.instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleAdFailedToShow)}");
        PlatformInterface.instance.logger.Log($"    Message: {errorMessage}");
        UnityMainThreadDispatcher.Instance().Enqueue(() => PlatformAdMobAds.HandleFailedToLoad(errorMessage));
    }

    void HandleAdOpening(object sender, EventArgs args) {
        PlatformInterface.instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleAdOpening)}");
        shouldBeRewarded = false;
    }

    void HandleAdClosed(object sender, EventArgs args) {
        PlatformInterface.instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleAdClosed)}");
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            RequestRewardBasedVideo();
            if (shouldBeRewarded) {
                platformAdMobAds.HandleRewarded();
            }
        });
    }

    void HandleUserEarnedReward(object sender, Reward args) {
        PlatformInterface.instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleUserEarnedReward)}");
        string type = args.Type;
        double amount = args.Amount;
        PlatformInterface.instance.logger.Log($"HandleRewardBasedVideoRewarded event received for {amount} {type}");
        shouldBeRewarded = true;
    }

    void RequestRewardBasedVideo() {
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded video ad with the request.
        rewardBasedVideo.LoadAd(request);
    }

    public static void TestMediation() {
        // Google Mobile Ads 업데이트되면서 더이상 지원되지 않는건가?
    }
#endif
}