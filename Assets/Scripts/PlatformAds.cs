using UnityEngine;

public class PlatformAds : IPlatformAds
{
    public enum AdsType
    {
        AdMob,
        UnityAds,
        FacebookAudienceNetwork
    }

    public void HandleRewarded(object adContext)
    {
        if (adContext is BlackAdContext blackAdContext)
            HandleRewarded(blackAdContext.value);
        else
            Debug.LogError("Not BlackAdContext type!");
    }

    public static void HandleRewarded()
    {
    }

    public static void ExecuteBackgroundTimeCompensationForAds()
    {
    }
}