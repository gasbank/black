using System;
using Dirichlet.Numerics;
using UnityEngine;

[Serializable]
public class AchievementRecord5
{
    [NonSerialized]
    public bool leaderboard;

    [SerializeField]
    ScUInt128 maxBlackLevel = 0;

    public AchievementRecord5(bool leaderboard)
    {
        this.leaderboard = leaderboard;
    }

    public UInt128 MaxBlackLevel
    {
        get => maxBlackLevel;
        set
        {
            maxBlackLevel = value;
            AchievementPopup.instance.UpdateAchievementTab("maxBlackLevel");
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
}