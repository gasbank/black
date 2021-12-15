using System.Collections.Generic;
using Dirichlet.Numerics;
using UnityEngine;

public interface IBlackContext
{
    bool CheatMode { get; set; }
    bool WaiveBan { get; set; }
    UInt128 Rice { get; }
    UInt128 PendingRice { get; }
    UInt128 Gem { get; }
    UInt128 FreeGem { get; }
    UInt128 PaidGem { get; }
    UInt128 PendingFreeGem { get; set; }
    GameObject AchievementNewImage { get; }
    AchievementRecord5 AchievementGathered { get; set; }
    AchievementRecord5 AchievementRedeemed { get; set; }
    ScLong LastDailyRewardRedeemedIndex { get; set; }
    ScLong LastDailyRewardRedeemedTicks { get; set; }
    ScInt UserPseudoId { get; set; }
    ScInt LastConsumedServiceIndex { get; set; }
    bool SaveFileLoaded { get; set; }
    NoticeData NoticeData { get; set; }
    bool LoadedAtLeastOnce { get; set; }
    List<ScInt> WhacACatStageClearLevelList { get; set; }
    HashSet<string> AlreadyUnlockedPlatformAchievementSet { get; set; }
    GameObject UserEmergency { get; }
    ScInt PlayTimeSec { get; set; }
    Dictionary<string, LocalUserData> LocalUserDict { get; set; }

    ScInt LastClearedStageId { get; set; }
    List<ScFloat> StageClearTimeList { get; set; }
    bool NextStagePurchased { get; set; }
    ScInt CoinAmount { get; set; }
    ScLong LastFreeCoinRefilledTicks { get; set; }
    bool SlowMode { get; set; }
    ScInt CoinUseCount { get; set; }
    bool LastStageFailed { get; set; }
    List<ScString> StashedRewardJsonList { get; set; }
    List<ScLong> LastDailyRewardRedeemedTicksList { get; set; }
    int NoAdsCode { get; set; }
    Canvas[] CriticalErrorHiddenCanvasList { get; }
    bool IsBigPopupOpened { get; set; }
    Transform AnimatedIncrementParent { get; set; }
    void RefreshRiceText();
    void UpdateLastTouchTime();
    void AddFreeGem(UInt128 delta);
    void AddPaidGem(UInt128 delta);
    void SubtractGem(UInt128 delta);
    void SetGemZero();
    void RefreshGemText();
    void AddPendingRice(UInt128 delta);
    void ApplyPendingRice();
    void AddPendingFreeGem(UInt128 delta);
    void ApplyPendingFreeGem();
    void UpdateDailyRewardAllButtonStates();

    void UnlockPlatformAchievement(string achievementGroup, long newValue);
    void UpdateDailyRewardPopup();
    void SetRice(UInt128 v);
    void SetPendingRice(UInt128 v);
    void AddRiceSafe(UInt128 v);
    void SubtractRice(UInt128 v);
    void OpenBigPopup(CanvasGroup canvasGroup);
    void CloseBigPopup(CanvasGroup canvasGroup);

    void IncreaseGemAnimated(ScLong achievementDataRewardGem, GameObject clonedGameObject,
        BlackLogEntry.Type gemAddAchievement, ScInt achievementDataId);

    void GetAllDailyRewardsAtOnceAdminToDay(int toDay);
}