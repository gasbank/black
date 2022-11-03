using System.Text;
using UnityEngine;

public class PlatformAdMobAds : MonoBehaviour
{
    public static PlatformAdMobAds instance;

#if GOOGLE_MOBILE_ADS
    object adContext;
#endif

    static string lastErrorMessage;

    public void TryShowRewardedAd(object adContext)
    {
#if GOOGLE_MOBILE_ADS
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            PlatformInterface.instance.confirmPopup.Open(PlatformInterface.instance.text.Str_InternetRequiredForAds);
        }
        else if (PlatformAdMobAdsInit.instance.rewardBasedVideo.IsLoaded())
        {
            this.adContext = adContext;
            PlatformAdMobAdsInit.instance.rewardBasedVideo.Show();
        }
        else
        {
            Debug.LogError("Ad not ready!");
            ShowAdsErrorPopup();
        }
#endif
    }

    public void HandleRewarded()
    {
#if GOOGLE_MOBILE_ADS
        PlatformInterface.instance.ads.HandleRewarded(adContext);
#endif
    }

    internal static void HandleFailedToLoad(string errorMessage)
    {
        Debug.LogError("HandleFailedToLoad");
        // 유저에게 광고를 못불러왔다는 걸 굳이 게임 시작할 때 보여줄 필요는 없지...
        //PlatformInterface.instance.shortMessage.Show("\\광고 불러오기를 실패했습니다.".Localized());
        lastErrorMessage = errorMessage;
    }

    static void ShowAdsErrorPopup()
    {
        var sb = new StringBuilder(PlatformInterface.instance.text.Str_AdMobError);
        sb.AppendLine(lastErrorMessage);
#if DEV_BUILD
        sb.AppendLine("Try enabling Test Ad toggle in Admin");
#endif
        PlatformInterface.instance.confirmPopup.Open(sb.ToString());
    }
}