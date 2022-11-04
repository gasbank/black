using System;
using Dirichlet.Numerics;
using UnityEngine;

[Serializable]
public class AchievementRecord1
{
    [NonSerialized]
    public bool leaderboard;

    [SerializeField]
    ScUInt128 maxBlackLevel = 0;

    [SerializeField]
    ScUInt128 maxColoringCombo = 0;

    public AchievementRecord1(bool leaderboard)
    {
        this.leaderboard = leaderboard;
    }

    public UInt128 MaxBlackLevel
    {
        get => maxBlackLevel;
        set
        {
            maxBlackLevel = value;
            // AchievementPopup.Instance.UpdateAchievementTab("maxBlackLevel");
            AchievePopup.Instance.UpdateAchievementProgress("maxBlackLevel");
            if (leaderboard)
                try
                {
                    BlackLeaderboard.Instance.UpdateLeaderboard_LastClearedStageId(value.ToClampedLong());
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.ToString());
                }
        }
    }

    public UInt128 MaxColoringCombo
    {
        get => maxColoringCombo;
        set
        {
            maxColoringCombo = value;
            AchievePopup.Instance.UpdateAchievementProgress("maxColoringCombo");
        }
    }

    public UInt128 GetValue(string key)
    {
        return key switch
        {
            "MaxBlackLevel" => MaxBlackLevel,
            "MaxColoringCombo" => MaxColoringCombo,
            _ => throw new NotImplementedException()
        };
    }
}