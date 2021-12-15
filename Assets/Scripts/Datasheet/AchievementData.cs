using System;
using MessagePack;

[Serializable]
[MessagePackObject]
public class AchievementData
{
    [Key(10)]
    public ScString androidAchievementKey;

    [Key(4)]
    public ScString condition;

    [Key(6)]
    public ScLong conditionNewArg;

    [Key(5)]
    public ScLong conditionOldArg;

    [Key(3)]
    public ScString desc;

    [Key(0)]
    public ScInt id;

    [Key(11)]
    public ScString iosAchievementKey;

    [Key(9)]
    public bool isPlatformAchievement;

    [Key(2)]
    public ScString name;

    [Key(8)]
    public ScLong rewardGem;

    [Key(7)]
    public ScInt rewardGemMultiplier;

    [Key(1)]
    public ScString sprite;

    [IgnoreMember]
    public string RewardGemString
    {
        get
        {
            if (rewardGemMultiplier <= 1)
                return string.Format("+{0:n0}", rewardGem.ToLong().Postfixed());
            return "\\+{0:n0} {1}배 보너스".Localized((rewardGem.ToLong() / rewardGemMultiplier).Postfixed(),
                rewardGemMultiplier);
        }
    }
}