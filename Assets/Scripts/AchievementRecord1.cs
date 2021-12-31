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
            // AchievementPopup.instance.UpdateAchievementTab("maxBlackLevel");
            AchievePopup.instance.UpdateAchievementProgress("maxBlackLevel");
            if (leaderboard)
                try
                {
                    BlackLeaderboard.instance.UpdateLeaderboard_LastClearedStageId(value.ToClampedLong());
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
            AchievePopup.instance.UpdateAchievementProgress("maxColoringCombo");
        }
    }
}