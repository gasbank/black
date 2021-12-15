using System;
using System.Collections.Generic;
using ConditionalDebug;
using Dirichlet.Numerics;
using UnityEngine;

public class BlackContext : MonoBehaviour, IBlackContext
{
    public static IBlackContext instance;

    [SerializeField]
    ScUInt128 rice;

    [SerializeField]
    ScUInt128 pendingRice;

    [SerializeField]
    ScUInt128 freeGem;

    [SerializeField]
    ScUInt128 paidGem;

    [SerializeField]
    ScUInt128 pendingFreeGem;

    [SerializeField]
    ScInt lastClearedStageId;

    [SerializeField]
    PlatformLocalNotification platformLocalNotification;
    
    public bool CheatMode { get; set; }
    public bool WaiveBan { get; set; }

    public UInt128 Rice
    {
        get => rice;
        private set => rice = value;
    }

    public UInt128 PendingRice
    {
        get => pendingRice;
        private set => pendingRice = value;
    }

    public void RefreshRiceText()
    {
        throw new NotImplementedException();
    }

    public void UpdateLastTouchTime()
    {
        throw new NotImplementedException();
    }

    public UInt128 Gem => FreeGem + PaidGem;
    public UInt128 FreeGem => freeGem;
    public UInt128 PaidGem => paidGem;

    public UInt128 PendingFreeGem
    {
        get => pendingFreeGem;
        set => pendingFreeGem = value;
    }

    public void AddFreeGem(UInt128 delta)
    {
        if (delta == 0)
        {
            // Do nothing
        }
        else if (delta > 0)
        {
            try
            {
                freeGem = freeGem.ToUInt128() + delta;
            }
            catch (OverflowException)
            {
                freeGem = UInt128.MaxValue;
                BlackLogManager.Add(BlackLogEntry.Type.GemAddOverflowFreeGem, 0, delta.ToClampedLong());
            }
        }
        else
        {
            // UInt128은 부호 없기 때문에 있을 수 없는 케이스!!!
            Debug.LogError("AddFreeGem with negative value");
        }

        RefreshGemText();
    }

    public void AddPaidGem(UInt128 delta)
    {
        if (delta == 0)
        {
            // Do nothing
        }
        else if (delta > 0)
        {
            try
            {
                paidGem = paidGem.ToUInt128() + delta;
            }
            catch (OverflowException)
            {
                paidGem = UInt128.MaxValue;
                BlackLogManager.Add(BlackLogEntry.Type.GemAddOverflowPaidGem, 0, delta.ToClampedLong());
            }
        }
        else
        {
            // UInt128은 부호 없기 때문에 있을 수 없는 케이스!!!
            Debug.LogError("AddPaidGem with negative value");
        }

        RefreshGemText();
    }

    public void SubtractGem(UInt128 delta)
    {
        if (delta == 0)
        {
            // Do nothing
        }
        else if (delta > 0)
        {
            if (freeGem.ToUInt128() >= delta)
            {
                // freeGem만으로 처리 가능한 경우
                freeGem = freeGem.ToUInt128() - delta;
            }
            else
            {
                // freeGem을 다 쓰고 paidGem이 추가로 필요한 경우
                var exceeded = delta - freeGem.ToUInt128();

                // 그런데 paidGem으로도 부족한 경우 (이런 일은 존재하지 않아야 한다.)
                if (paidGem.ToUInt128() < exceeded)
                {
                    throw new ArgumentOutOfRangeException();
                }

                freeGem = 0;
                paidGem = paidGem.ToUInt128() - exceeded;
            }
        }
        else
        {
            // UInt128은 부호 없기 때문에 있을 수 없는 케이스!!!
            Debug.LogError("SubtractGem with negative value");
        }

        RefreshGemText();
    }

    public void SetGemZero()
    {
        freeGem = 0;
        paidGem = 0;
    }

    public void RefreshGemText()
    {
    }

    public void AddPendingRice(UInt128 delta)
    {
        throw new NotImplementedException();
    }

    public void ApplyPendingRice()
    {
        ConDebug.Log($"ApplyPendingRice: {PendingRice}");
        AddRiceSafe(PendingRice);
        BlackLogManager.Add(BlackLogEntry.Type.RiceAddPending, 0,
            PendingRice < long.MaxValue ? (long) PendingRice : long.MaxValue);
        PendingRice = 0;
    }

    public void AddPendingFreeGem(UInt128 delta)
    {
        if (delta == 0)
        {
            // Do nothing
        }
        else if (delta > 0)
        {
            PendingFreeGem += delta;
        }
        else
        {
            Debug.LogError("AddPendingFreeGem with negative value");
        }
    }

    public void ApplyPendingFreeGem()
    {
        ConDebug.Log($"ApplyPendingFreeGem: {PendingFreeGem}");
        freeGem += new ScUInt128(PendingFreeGem);
        BlackLogManager.Add(BlackLogEntry.Type.GemAddPending, 0,
            PendingFreeGem < long.MaxValue ? (long) PendingFreeGem : long.MaxValue);
        PendingFreeGem = 0;
        ConDebug.Log($"ApplyPendingFreeGem after free gem becomes: {freeGem}");
    }

    public GameObject AchievementNewImage { get; }
    public AchievementRecord5 AchievementGathered { get; set; }
    public AchievementRecord5 AchievementRedeemed { get; set; }
    public ScLong LastDailyRewardRedeemedIndex { get; set; } = 0;
    public ScLong LastDailyRewardRedeemedTicks { get; set; } = DateTime.MinValue.Ticks;
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
        throw new NotImplementedException();
    }

    public void UnlockPlatformAchievement(string achievementGroup, long newValue)
    {
        throw new NotImplementedException();
    }

    public ScInt LastClearedStageId
    {
        get => lastClearedStageId;
        set => lastClearedStageId = value;
    }

    public List<ScFloat> StageClearTimeList { get; set; }
    public bool NextStagePurchased { get; set; }
    public ScInt CoinAmount { get; set; }
    public ScLong LastFreeCoinRefilledTicks { get; set; }
    public bool SlowMode { get; set; }
    public ScInt CoinUseCount { get; set; }
    public bool LastStageFailed { get; set; }
    public List<ScString> StashedRewardJsonList { get; set; }
    public List<ScLong> LastDailyRewardRedeemedTicksList { get; set; }
    public int NoAdsCode { get; set; }

    public void UpdateDailyRewardPopup()
    {
        throw new NotImplementedException();
    }

    public void SetRice(UInt128 v)
    {
        rice = v;
    }

    public void SetPendingRice(UInt128 v)
    {
        pendingRice = v;
    }

    public void AddRiceSafe(UInt128 v)
    {
        try
        {
            Rice += v;
        }
        catch (OverflowException)
        {
            Rice = UInt128.MaxValue;
        }
    }

    public void SubtractRice(UInt128 v)
    {
        if (Rice >= v)
        {
            Rice -= v;
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    public Canvas[] CriticalErrorHiddenCanvasList { get; }
    public bool IsBigPopupOpened { get; set; }
    public Transform AnimatedIncrementParent { get; set; }

    public void OpenBigPopup(CanvasGroup canvasGroup)
    {
        throw new NotImplementedException();
    }

    public void CloseBigPopup(CanvasGroup canvasGroup)
    {
        throw new NotImplementedException();
    }

    public void IncreaseGemAnimated(ScLong achievementDataRewardGem, GameObject clonedGameObject,
        BlackLogEntry.Type gemAddAchievement,
        ScInt achievementDataId)
    {
        throw new NotImplementedException();
    }

    public void GetAllDailyRewardsAtOnceAdminToDay(int toDay)
    {
        throw new NotImplementedException();
    }

    void OnApplicationQuit()
    {
        SaveLoadManager.instance.Save(this, ConfigPopup.instance, Sound.instance, Data.instance);
    }
    
    void OnApplicationPause(bool pause) {
        ConDebug.Log($"SushiSpawner.OnApplicationPause({pause})");
        if (pause) {
            // 백그라운드 상태가 되기 시작할 때 호출된다.
            SaveLoadManager.instance.Save(this, ConfigPopup.instance, Sound.instance, Data.instance);

            platformLocalNotification.RegisterAllRepeatingNotifications();

            // 게임이 제대로 시작한 이후부터만 백그라운드 처리 보상이 작동해도 된다.
            if (LoadedAtLeastOnce) {
                BackgroundTimeCompensator.instance.BeginBackgroundState(this);
            }
        } else {
            // 백그라운드 상태가 끝나고 호출된다.
            // 게임이 최초로 실행되는 단계에서도 한번 호출되는 것 같다.
            platformLocalNotification.RemoveAllRepeatingNotifications();

            // 게임이 제대로 시작한 이후부터만 백그라운드 처리 보상이 작동해도 된다.
            if (LoadedAtLeastOnce) {
                BackgroundTimeCompensator.instance.EndBackgroundState(this);
            }
        }
    }
}