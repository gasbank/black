using UnityEngine;
using UInt128 = Dirichlet.Numerics.UInt128;

[System.Serializable]
public class AchievementRecord5
{
    public AchievementRecord5(bool leaderboard)
    {
        this.leaderboard = leaderboard;
    }

    [System.NonSerialized]
    public bool leaderboard;

    [SerializeField]
    ScUInt128 maxBlackLevel = 0;

    public UInt128 MaxBlackLevel
    {
        get { return maxBlackLevel; }
        set
        {
            maxBlackLevel = value;
            AchievementPopup.instance.UpdateAchievementTab("maxBlackLevel");
            if (leaderboard)
            {
                try
                {
                    BlackLeaderboard.instance.UpdateLeaderboard_LastClearedStageId(value.ToClampedLong());
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(e.ToString());
                }
            }
        }
    }
}