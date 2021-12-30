using System.Collections.Generic;
using ConditionalDebug;
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

    }

    [UsedImplicitly]
    void ClosePopup()
    {

    }

    public void Test()
    {
        var entries = scrollViewRect.GetComponentsInChildren<AchievementEntry>(true);
        ConDebug.Log(entries);
        foreach (var entry in entries)
        {
            ConDebug.Log(entry);
        }

        var group = Data.achievementOrderedGroup;
        ConDebug.Log(group);
        ConDebug.Log(group.Keys.Count);
        ConDebug.Log(group.Keys);
        ConDebug.Log(group.Values.Count);
        ConDebug.Log(group.Values);

        foreach (var elem in group)
        {
            ConDebug.Log(elem);
        }
        
        UpdateAchievementUI();
    }

    public void UpdateAchievementUI()
    {
        if (BlackContext.instance == null) return;
        if (Data.dataSet == null) return;
        
        // short names
        var gathered = BlackContext.instance.AchievementGathered;
        var redeemed = BlackContext.instance.AchievementRedeemed;
        var group = Data.achievementOrderedGroup;

        ConDebug.Log(gathered);
        ConDebug.Log(gathered.MaxBlackLevel);
        ConDebug.Log(gathered.MaxColoringCombo);
        ConDebug.Log(redeemed);
    }
    
    public void UpdateAchievementProgress(string updateGroupKey = "")
    {
        throw new System.NotImplementedException();
    }
}