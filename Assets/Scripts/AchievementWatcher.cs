﻿using System;
using System.Collections.Generic;
using Dirichlet.Numerics;
using UnityEngine;

public class AchievementWatcher : MonoBehaviour
{
    [SerializeField]
    string conditionName;

    UInt128 currentValue;
    Dictionary<string, List<AchievementData>> achievementsDict;

    bool IsFine()
    {
        if (conditionName.Length == 0) return false;

        if (Data.dataSet == null) return false;

        if (BlackContext.Instance == null) return false;
        if (BlackContext.Instance.AchievementGathered == null) return false;
        if (BlackContext.Instance.AchievementRedeemed == null) return false;

        return true;
    }

    void UpdateAchievementProgress()
    {
        if (!IsFine()) return;
        if (!achievementsDict.ContainsKey(conditionName)) return;

        var result = achievementsDict[conditionName].GetAvailableAchievement(
            (UInt128)BlackContext.Instance.StageCombo.ToInt(), currentValue);

        if (result == null) return;

        currentValue = (UInt128)BlackContext.Instance.StageCombo.ToInt();

        var name = result.Item1.name;
        var title = name.Localized(result.Item1.conditionOldArg.ToLong().Postfixed(),
            result.Item1.conditionNewArg.ToLong().Postfixed());

        //var desc = result.Item1.desc;
        //var descMsg = desc.Localized(result.Item1.conditionOldArg.ToLong().Postfixed(),
        //    result.Item1.conditionNewArg.ToLong().Postfixed());

        ToastMessage.Instance.PlayGoodAnim(title);
    }

    private void Start()
    {
        switch (conditionName)
        {
            case "MaxBlackLevel":
                currentValue = BlackContext.Instance.AchievementGathered.MaxBlackLevel;
                break;
            case "MaxColoringCombo":
                currentValue = BlackContext.Instance.AchievementGathered.MaxColoringCombo;
                break;
            default:
                break;
        }

        achievementsDict = new Dictionary<string, List<AchievementData>>
        {
            {
                "MaxBlackLevel",
                Data.dataSet.AchievementData_MaxBlackLevel
            },
            {
                "MaxColoringCombo",
                Data.dataSet.AchievementData_MaxColoringCombo
            }
        };

        Debug.Log("CurrentValue: " + currentValue);
    }

    private void Update()
    {
        UpdateAchievementProgress();
    }

}
