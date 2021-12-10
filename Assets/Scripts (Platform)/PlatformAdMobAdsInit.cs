﻿#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif
#if GOOGLE_MOBILE_ADS
using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
#endif

using UnityEngine;

[DisallowMultipleComponent]
public class PlatformAdMobAdsInit : MonoBehaviour {
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
            "85fa748f8cae1b3212c1124dd5ef8aa9" // iPhone 6s
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

#if UNITY_IOS
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
        PlatformInterface.instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleAdFailedToLoad)}");
        PlatformInterface.instance.logger.Log($"    Message: {args.LoadAdError.GetMessage()}");
        UnityMainThreadDispatcher.Instance().Enqueue(PlatformAdMobAds.HandleFailedToLoad);
    }
    
    void HandleAdFailedToShow(object sender, AdErrorEventArgs args) {
        PlatformInterface.instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleAdFailedToShow)}");
        PlatformInterface.instance.logger.Log($"    Message: {args.AdError.GetMessage()}");
        UnityMainThreadDispatcher.Instance().Enqueue(PlatformAdMobAds.HandleFailedToLoad);
    }

    void HandleAdOpening(object sender, EventArgs args) {
        PlatformInterface.instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleAdOpening)}");
        shouldBeRewarded = false;
        // 게임 시간 멈추는 주관은 <see cref="BackgroundTimeCompensator.OnBackgrounded(MonoBehaviour)"/>에서 하도록 한다.
        //Sound.instance.StopTimeAndMuteAudioMixer();
        UnityMainThreadDispatcher.Instance().Enqueue(() => PlatformInterface.instance.backgroundTimeCompensator.BeginBackgroundState(this));
    }

    void HandleAdClosed(object sender, EventArgs args) {
        PlatformInterface.instance.logger.Log($"{nameof(PlatformAdMobAdsInit)}.{nameof(HandleAdClosed)}");
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            RequestRewardBasedVideo();
            if (shouldBeRewarded) {
                platformAdMobAds.HandleRewarded();
            }

            // 게임 시간 정상으로 되돌리는 주관은 <see cref="BackgroundTimeCompensator.OnForegrounded(MonoBehaviour)"/>에서 하도록 한다.
            //Sound.instance.ResumeToNormalTimeAndResumeAudioMixer();
            PlatformInterface.instance.backgroundTimeCompensator.EndBackgroundState(this);
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