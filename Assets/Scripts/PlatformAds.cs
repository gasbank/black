using UnityEngine;

public class PlatformAds : IPlatformAds
{
    public void HandleRewarded(object adContext)
    {
        if (adContext is BlackAdContext blackAdContext)
        {
            var goldAmount = blackAdContext.value;
            BlackContext.instance.AddGoldSafe(1);
            ConfirmPopup.instance.Open(@"\광고 시청 보상으로 {0}골드를 받았습니다.".Localized(goldAmount));
        }
        else
        {
            Debug.LogError("Not BlackAdContext type!");
        }
    }
}