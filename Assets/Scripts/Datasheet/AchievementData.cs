using MessagePack;

[System.Serializable]
[MessagePackObject]
public class AchievementData {
    [Key(0)] public ScInt id;
    [Key(1)] public ScString sprite;
    [Key(2)] public ScString name;
    [Key(3)] public ScString desc;
    [Key(4)] public ScString condition;
    [Key(5)] public ScLong conditionOldArg;
    [Key(6)] public ScLong conditionNewArg;
    [Key(7)] public ScInt rewardGemMultiplier;
    [Key(8)] public ScLong rewardGem;
    [Key(9)] public bool isPlatformAchievement;
    [Key(10)] public ScString androidAchievementKey;
    [Key(11)] public ScString iosAchievementKey;

    [IgnoreMember]
    public string RewardGemString {
        get {
            if (rewardGemMultiplier <= 1) {
                return string.Format("+{0:n0}", rewardGem.ToLong().Postfixed());
            } else {
                return "\\+{0:n0} {1}배 보너스".Localized((rewardGem.ToLong() / rewardGemMultiplier).Postfixed(), rewardGemMultiplier);
            }
        }
    }
}
