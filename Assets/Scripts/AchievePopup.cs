using System.Collections.Generic;
using ConditionalDebug;
using Dirichlet.Numerics;
using JetBrains.Annotations;
using UnityEngine;

public class AchievePopup : MonoBehaviour
{
    public static AchievePopup instance;

    // 표시 순서 때문에 순서를 정할 목적으로 만든 배열
    static readonly string[] groupKeyArray =
    {
        "lastClearedStageId"
    };
    
    public RectTransform scrollViewRect;

    [SerializeField]
    Subcanvas subcanvas;

#if UNITY_EDITOR
    void OnValidate()
    {
        subcanvas = GetComponent<Subcanvas>();
    }
#endif

    [UsedImplicitly]
    void OpenPopup()
    {
        UpdateAchievementUI();
    }

    [UsedImplicitly]
    void ClosePopup()
    {

    }

    void UpdateAchievementUI()
    {
        if (BlackContext.instance == null) return;
        if (Data.dataSet == null) return;
        if (subcanvas.IsOpen == false) return;
        
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
                    rewardGemMultiplier = 1,
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
                    rewardGemMultiplier = 1,
                }
            }},
        };

        ConDebug.Log("MaxBlackLevel");
        ConDebug.Log(BlackContext.instance.AchievementGathered.MaxBlackLevel);
        ConDebug.Log(BlackContext.instance.AchievementRedeemed.MaxBlackLevel);

        ConDebug.Log("MaxColoringCombo");
        ConDebug.Log(BlackContext.instance.AchievementGathered.MaxColoringCombo);
        ConDebug.Log(BlackContext.instance.AchievementRedeemed.MaxColoringCombo);
        
        var entries = scrollViewRect.GetComponentsInChildren<AchievementEntry>(true);
        UpdateEntryUI(entries[0], group["maxBlackLevel"][0]);
        if (BlackContext.instance.AchievementGathered.MaxBlackLevel >= 1 &&
            BlackContext.instance.AchievementRedeemed.MaxBlackLevel < 1)
            EnableEntryUI(entries[0]);
        else if (BlackContext.instance.AchievementRedeemed.MaxBlackLevel >= 30)
            entries[0].gameObject.SetActive(false);
        
        UpdateEntryUI(entries[1], group["maxColoringCombo"][0]);
        if (BlackContext.instance.AchievementGathered.MaxColoringCombo >= 5 &&
            BlackContext.instance.AchievementRedeemed.MaxColoringCombo < 5)
            EnableEntryUI(entries[1]);
        else if (BlackContext.instance.AchievementRedeemed.MaxColoringCombo >= 50)
            entries[1].gameObject.SetActive(false);
    }

    void UpdateEntryUI(AchievementEntry entry, AchievementData data)
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
        entry.redeemButton.interactable = false;
        entry.rewardGemText.text = data.RewardGemString;
        entry.achievementData = data;
        entry.rewardGemText.gameObject.SetActive(false);
        entry.rewardGemText.gameObject.SetActive(true);
    }

    private void EnableEntryUI(AchievementEntry entry)
    {
        entry.redeemButton.interactable = true;
    }

    public void UpdateAchievementProgress(string updateGroupKey = "")
    {
        UpdateAchievementUI();
    }
}