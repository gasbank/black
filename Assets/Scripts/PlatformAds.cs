using UnityEngine;

public class PlatformAds : IPlatformAds
{
    public void HandleRewarded(object adContext)
    {
        if (adContext is BlackAdContext blackAdContext)
        {
            blackAdContext.ExecuteReward();
        }
        else
        {
            Debug.LogError("Not BlackAdContext type!");
        }
    }
}