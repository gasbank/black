using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class AchievementEntry : MonoBehaviour
{
    public Image image;
    public Text achievementName;
    public Text achievementDesc;
    public Button redeemButton;
    public Text rewardGemText;
    public AchievementData achievementData;

    [SerializeField]
    Image rewardGemImage;

    [SerializeField]
    AchievementRedeemButton achievementRedeemButton;

    // 버튼 콜백
    public void Redeem()
    {
        if (achievementRedeemButton != null && achievementRedeemButton.RepeatedThresholdSatisfied)
        {
            // 연속 구매 상태에서 터치 종료 시 구매가 하나 더 추가되는 증상을 막자.
        }
        else
        {
            RedeemInternal();
        }
    }

    public void RedeemInternal()
    {
        // 비활성화됐다는 뜻은 보상 받을 조건 만족하지 못했단 뜻이다.
        if (redeemButton.interactable == false)
        {
            return;
        }

        // add reward
        var clonedGameObject = InstantiateLocalized.InstantiateLocalize(rewardGemImage.gameObject,
            BlackContext.instance.AnimatedIncrementParent, true);
        BlackContext.instance.AddPendingFreeGem((ulong) achievementData.rewardGem.ToLong());
        BlackContext.instance.IncreaseGemAnimated(achievementData.rewardGem, clonedGameObject,
            BlackLogEntry.Type.GemAddAchievement, achievementData.id);
        Sound.instance.PlaySoftTada();

        // update redeemed stat
        if (achievementData.condition == "maxBlackLevel")
        {
            BlackContext.instance.AchievementRedeemed.MaxBlackLevel = (ulong) achievementData.conditionNewArg.ToLong();
        }
        else
        {
            Debug.LogErrorFormat("Unknown achievement condition: {0}", achievementData.condition);
        }

        // refresh achievement popup
        // *** 여기서 다시 갱신할 필요는 없다. BlackContext.instance.achievementRedeemed의 프로퍼티가 변경될 때
        // *** 암묵적으로 갱신이 된다. 여기서 하면 같은 일을 두 번 하는 것이다.
        // GetComponentInParent<AchievementPopup>().UpdateAchievementTab();
    }
}