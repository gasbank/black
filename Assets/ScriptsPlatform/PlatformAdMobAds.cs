using System.Text;
using UnityEngine;

public class PlatformAdMobAds : MonoBehaviour
{
    public static PlatformAdMobAds Instance;

#if GOOGLE_MOBILE_ADS
    object adContext;
#endif

    static string lastErrorMessage;

    public void TryShowRewardedAd(object adContext)
    {
#if GOOGLE_MOBILE_ADS
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            PlatformInterface.Instance.confirmPopup.Open(PlatformInterface.Instance.text.Str_InternetRequiredForAds);
        }
        else if (PlatformAdMobAdsInit.Instance.RewardBasedVideo.IsLoaded())
        {
            this.adContext = adContext;
            PlatformAdMobAdsInit.Instance.RewardBasedVideo.Show();
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
        PlatformInterface.Instance.ads.HandleRewarded(adContext);
#endif
    }

    internal static void HandleFailedToLoad(string errorMessage)
    {
        Debug.LogError("HandleFailedToLoad");
        // 유저에게 광고를 못불러왔다는 걸 굳이 게임 시작할 때 보여줄 필요는 없지...
        //PlatformInterface.Instance.shortMessage.Show("\\광고 불러오기를 실패했습니다.".Localized());
        lastErrorMessage = errorMessage;
    }

    static void ShowAdsErrorPopup()
    {
        var sb = new StringBuilder(PlatformInterface.Instance.text.Str_AdMobError);
        sb.AppendLine(lastErrorMessage);
#if DEV_BUILD
        sb.AppendLine("Try enabling Test Ad toggle in Admin");
#endif
        PlatformInterface.Instance.confirmPopup.Open(sb.ToString());
    }
}