using ConditionalDebug;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class AchievementEntry : MonoBehaviour
{
    public AchievementData achievementData;
    public Text achievementDesc;
    public Text achievementName;

    [SerializeField]
    AchievementRedeemButton achievementRedeemButton;

    public Image image;
    public Button redeemButton;

    [SerializeField]
    Image rewardGemImage;

    public Text rewardGemText;

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
        if (redeemButton.interactable == false) return;

        // add reward
        // TODO 애니메이션은 다음 빌드에 넣는다
        // var clonedGameObject = InstantiateLocalized.InstantiateLocalize(rewardGemImage.gameObject,
        //     BlackContext.Instance.AnimatedIncrementParent, true);
        BlackContext.Instance.AddGoldSafe((ulong) achievementData.rewardGem.ToLong());
        ConDebug.Log((ulong) achievementData.rewardGem.ToLong());

        // BlackContext.Instance.IncreaseGemAnimated(achievementData.rewardGem, clonedGameObject,
        //     BlackLogEntry.Type.GemAddAchievement, achievementData.id);
        Sound.Instance.PlaySoftTada();

        // update redeemed stat
        if (achievementData.condition == "maxBlackLevel")
            BlackContext.Instance.AchievementRedeemed.MaxBlackLevel = (ulong) achievementData.conditionNewArg.ToLong();
        else if (achievementData.condition == "maxColoringCombo")
            BlackContext.Instance.AchievementRedeemed.MaxColoringCombo = (ulong) achievementData.conditionNewArg.ToLong();
        else
            Debug.LogErrorFormat("Unknown achievement condition: {0}", achievementData.condition);

        // refresh achievement popup
        // *** 여기서 다시 갱신할 필요는 없다. BlackContext.Instance.achievementRedeemed의 프로퍼티가 변경될 때
        // *** 암묵적으로 갱신이 된다. 여기서 하면 같은 일을 두 번 하는 것이다.
        // GetComponentInParent<AchievementPopup>().UpdateAchievementTab();
        SaveLoadManager.Save(BlackContext.Instance, ConfigPopup.Instance, Sound.Instance, Data.Instance, null);
    }
}