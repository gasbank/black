using System.Collections.Generic;
using Dirichlet.Numerics;
using UnityEngine;

public class BlackContext : MonoBehaviour, IBlackContext
{
    public static IBlackContext instance;
    public bool CheatMode { get; set; }
    public bool WaiveBan { get; set; }
    public UInt128 Rice { get; }
    public UInt128 PendingRice { get; }
    public void RefreshRiceText()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateLastTouchTime()
    {
        throw new System.NotImplementedException();
    }

    public UInt128 Gem { get; }
    public UInt128 FreeGem { get; }
    public UInt128 PaidGem { get; }
    public UInt128 PendingFreeGem { get; set; }
    public void AddFreeGem(UInt128 delta)
    {
        throw new System.NotImplementedException();
    }

    public void AddPaidGem(UInt128 delta)
    {
        throw new System.NotImplementedException();
    }

    public void SubtractGem(UInt128 delta)
    {
        throw new System.NotImplementedException();
    }

    public void SetGemZero()
    {
        throw new System.NotImplementedException();
    }

    public void RefreshGemText()
    {
        throw new System.NotImplementedException();
    }

    public void AddPendingRice(UInt128 delta)
    {
        throw new System.NotImplementedException();
    }

    public void ApplyPendingRice()
    {
        throw new System.NotImplementedException();
    }

    public void AddPendingFreeGem(UInt128 delta)
    {
        throw new System.NotImplementedException();
    }

    public void ApplyPendingFreeGem()
    {
        throw new System.NotImplementedException();
    }

    public GameObject AchievementNewImage { get; }
    public AchievementRecord5 AchievementGathered { get; set; }
    public AchievementRecord5 AchievementRedeemed { get; set; }
    public ScLong LastDailyRewardRedeemedIndex { get; set; }
    public ScLong LastDailyRewardRedeemedTicks { get; set; }
    public ScInt UserPseudoId { get; set; }
    public ScInt LastConsumedServiceIndex { get; set; }
    public bool SaveFileLoaded { get; set; }
    public NoticeData NoticeData { get; set; }
    public bool LoadedAtLeastOnce { get; set; }
    public List<ScInt> WhacACatStageClearLevelList { get; set; }
    public HashSet<string> AlreadyUnlockedPlatformAchievementSet { get; set; }
    public GameObject UserEmergency { get; }
    public ScInt PlayTimeSec { get; set; }
    public Dictionary<string, LocalUserData> LocalUserDict { get; set; }
    public void UpdateDailyRewardAllButtonStates()
    {
        throw new System.NotImplementedException();
    }

    public void UnlockPlatformAchievement(string achievementGroup, long newValue)
    {
        throw new System.NotImplementedException();
    }

    public ScInt MahjongLastClearedStageId { get; set; }
    public List<ScFloat> MahjongClearTimeList { get; set; }
    public bool MahjongNextStagePurchased { get; set; }
    public ScInt MahjongCoinAmount { get; set; }
    public ScLong LastFreeMahjongCoinRefilledTicks { get; set; }
    public bool MahjongSlowMode { get; set; }
    public ScInt MahjongCoinUseCount { get; set; }
    public bool MahjongLastStageFailed { get; set; }
    public List<ScString> StashedRewardJsonList { get; set; }
    public List<ScLong> LastDailyRewardRedeemedTicksList { get; set; }
    public int NoAdsCode { get; set; }
    public void UpdateDailyRewardPopup()
    {
        throw new System.NotImplementedException();
    }

    public void SetRice(UInt128 v)
    {
        throw new System.NotImplementedException();
    }

    public void SetPendingRice(UInt128 v)
    {
        throw new System.NotImplementedException();
    }

    public void AddRiceSafe(UInt128 v)
    {
        throw new System.NotImplementedException();
    }

    public void SubtractRice(UInt128 v)
    {
        throw new System.NotImplementedException();
    }

    public Canvas[] CriticalErrorHiddenCanvasList { get; }
    public bool IsBigPopupOpened { get; set; }
    public Transform AnimatedIncrementParent { get; set; }
    public void OpenBigPopup(CanvasGroup canvasGroup)
    {
        throw new System.NotImplementedException();
    }

    public void CloseBigPopup(CanvasGroup canvasGroup)
    {
        throw new System.NotImplementedException();
    }

    public void IncreaseGemAnimated(ScLong achievementDataRewardGem, GameObject clonedGameObject, BlackLogEntry.Type gemAddAchievement,
        ScInt achievementDataId)
    {
        throw new System.NotImplementedException();
    }

    public void GetAllDailyRewardsAtOnceAdminToDay(int toDay)
    {
        throw new System.NotImplementedException();
    }
}