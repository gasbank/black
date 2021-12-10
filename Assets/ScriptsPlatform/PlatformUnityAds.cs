#if UNITY_ADS
using UnityEngine;
using UnityEngine.Advertisements;

public static class PlatformUnityAds {
    static object adContext;

    public static void TryShowRewardedAd(object adContext) {
        if (Application.internetReachability == NetworkReachability.NotReachable) {
            PlatformInterface.confirmPopup.Open(PlatformInterface.text.Str_InternetRequiredForAds);
        } else if (Advertisement.IsReady(PlatformUnityAdsInit.PLACEMENT_ID)) {
            PlatformUnityAds.adContext = adContext;
            Advertisement.Show(PlatformUnityAdsInit.PLACEMENT_ID);
        } else {
            Debug.LogError("Ad not ready!");
            ShowAdsErrorPopup("");
        }
    }

    public static void HandleShowResult(ShowResult result) {
        switch (result) {
            case ShowResult.Finished:
                PlatformInterface.ads.HandleRewarded(adContext);
                break;
            case ShowResult.Skipped:
                PlatformInterface.logger.Log("The ad was skipped before reaching the end.");
                PlatformInterface.confirmPopup.Open(PlatformInterface.text.Str_AdsAborted);
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                ShowAdsErrorPopup("");
                break;
        }
    }

    public static void ShowAdsErrorPopup(string message) {
        PlatformInterface.confirmPopup.Open(PlatformInterface.text.Str_UnityAdsError + "\n" + message);
    }
}
#endif