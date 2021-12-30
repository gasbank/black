using System.Collections.Generic;
using ConditionalDebug;
using Dirichlet.Numerics;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class AchievePopup : MonoBehaviour
{
    public static AchievePopup instance;
    
    public RectTransform scrollViewRect;
    
    [UsedImplicitly]
    void OpenPopup()
    {
        UpdateAchievementUI();
    }

    [UsedImplicitly]
    void ClosePopup()
    {

    }

    public void UpdateAchievementUI()
    {
        if (BlackContext.instance == null) return;
        if (Data.dataSet == null) return;
        
        // short names
        var gathered = BlackContext.instance.AchievementGathered;
        var redeemed = BlackContext.instance.AchievementRedeemed;
        var group = Data.achievementOrderedGroup;
        
        // 임시데이터
        group = new Dictionary<ScString, List<AchievementData>>
        {
            {"maxBlackLevel", new List<AchievementData>
            {
                new AchievementData
                {
                    id = 100001,
                    name = "\\나는야 화가!",
                    desc = "\\채색의 달인이 되었다",
                    condition = "maxBlackLevel",
                    conditionOldArg = 0,
                    conditionNewArg = 30,
                    rewardGem = 1,
                }
            }},
            {"maxColoringCombo", new List<AchievementData>
            {
                new AchievementData
                {
                    id = 200001,
                    name = "\\색칠 50 콤보",
                    desc = "\\50번 연속 색칠 성공, 대단한 집중력!",
                    condition = "maxColoringCombo",
                    conditionOldArg = 0,
                    conditionNewArg = 50,
                    rewardGem = 1,
                }
            }},
        };
        
        var entries = scrollViewRect.GetComponentsInChildren<AchievementEntry>(true);
        UpdateEntryUI(entries[0], group["maxBlackLevel"][0]);
        UpdateEntryUI(entries[1], group["maxColoringCombo"][0]);
    }

    private void UpdateEntryUI(AchievementEntry entry, AchievementData data)
    {
        UInt128 currentValue = 0;
        entry.gameObject.SetActive(true);
        // entry.image.sprite = Atlas.Load(data.sprite);
        // entry.image.color = Color.white;
        entry.achievementName.text = data.name.Localized(data.conditionOldArg.ToLong().Postfixed(),
            data.conditionNewArg.ToLong().Postfixed());
        entry.achievementDesc.text = data.desc.Localized(data.conditionOldArg.ToLong().Postfixed(),
            data.conditionNewArg.ToLong().Postfixed(), currentValue.Postfixed());
        entry.redeemButton.gameObject.SetActive(true);
        entry.redeemButton.interactable = true;
        entry.rewardGemText.text = data.RewardGemString;
        entry.achievementData = data;
        entry.rewardGemText.gameObject.SetActive(false);
        entry.rewardGemText.gameObject.SetActive(true);
    }
    
    public void UpdateAchievementProgress(string updateGroupKey = "")
    {
        throw new System.NotImplementedException();
    }
}