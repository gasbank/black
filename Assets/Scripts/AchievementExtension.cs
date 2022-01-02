using System;
using System.Collections.Generic;
using System.Linq;
using Dirichlet.Numerics;


public static class AchievementExtension
{
    public static Tuple<AchievementData, UInt128> GetAvailableAchievement(this List<AchievementData> data,
        UInt128 gatheredValue, UInt128 redeemedValue)
    {
        var gatheredValueClamped = gatheredValue.ToClampedLong();
        var gatheredIndex = Data.AchievementData_ConditionNewArg_UpperBound(data, gatheredValueClamped) - 1;

        var redeemedValueClamped = redeemedValue.ToClampedLong();
        var nextRedeemIndex = Data.AchievementData_ConditionNewArg_UpperBound(data, redeemedValueClamped);

        if (nextRedeemIndex <= gatheredIndex && nextRedeemIndex < data.Count)
            return new Tuple<AchievementData, UInt128>(data[nextRedeemIndex], gatheredValue);

        return null;
    }

    public static Tuple<AchievementData, UInt128> GetOngoingAchievement(this List<AchievementData> data,
        UInt128 gatheredValue, UInt128 redeemedValue)
    {
        var ongoingAchievementData = data
            .FirstOrDefault(x => x.conditionOldArg <= gatheredValue && gatheredValue < x.conditionNewArg);
        return ongoingAchievementData != null ? new Tuple<AchievementData, UInt128>(ongoingAchievementData, gatheredValue) : null;
    }

}