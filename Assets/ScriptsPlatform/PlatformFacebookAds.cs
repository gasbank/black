using UnityEngine;

public class PlatformFacebookAds : MonoBehaviour
{
    object adContext;

    [SerializeField]
    PlatformAdMobAds platformAdMobAds;

    [SerializeField]
    PlatformInterface platformInterface;

    public void TryShowRewardedAd(object adContext)
    {
        //광고 시청을 실패했을때는 '찾고 있습니다' 자체가 나올 필요가 없을것.
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            PlatformInterface.instance.confirmPopup.Open(PlatformInterface.instance.text.Str_InternetRequiredForAds);
        }
        else
        {
            //뒤로가기 상태 활성화. 네트워크 통신인 경우에 대해서는 활성화 필요.
            PlatformInterface.instance.progressMessage.ForceBackButtonActive();

            PlatformInterface.instance.progressMessage.Open(PlatformInterface.instance.text.Str_SearchingAds);
            //Close Button Activate. 최초 광고 호출시 바로 호출.
            PlatformInterface.instance.progressMessage.CloseButtonPopup();

            PlatformFacebookAdsInit.instance.IsAdCurrentlyCalled = true;
            this.adContext = adContext;
            PlatformFacebookAdsInit.instance.LoadAndShowRewardedVideo();
        }
    }

    public void HandleShowResult(bool result, string message)
    {
        if (result)
            PlatformInterface.instance.ads.HandleRewarded(adContext);
        else // ShowAdsErrorPopup(message);
            // Facebook 광고 시청 실패했을 때는 조용히 2차로 Google AdMob 시도한다.
            platformAdMobAds.TryShowRewardedAd(adContext);

        PlatformInterface.instance.progressMessage.DisableCloseButton();
        PlatformInterface.instance.progressMessage.Close();
    }

    public void ShowAdsErrorPopup(string message)
    {
        PlatformInterface.instance.confirmPopup.Open(PlatformInterface.instance.text.Str_FacebookAdsError + "\n" +
                                                     message);
    }
}