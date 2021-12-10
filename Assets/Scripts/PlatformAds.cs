using ConditionalDebug;
using UnityEngine;

public class PlatformAds : IPlatformAds {
    public enum AdsType {
        AdMob,
        UnityAds,
        FacebookAudienceNetwork,
    }

    public static void HandleRewarded() {
        
    }

    static public void ExecuteBackgroundTimeCompensationForAds() {

    }

    public void HandleRewarded(object adContext) {
        if (adContext is BlackAdContext blackAdContext) {
            HandleRewarded(blackAdContext.value);
        } else {
            Debug.LogError("Not BlackAdContext type!");
        }
    }
}
