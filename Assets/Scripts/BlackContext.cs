using System;
using System.Collections.Generic;
using ConditionalDebug;
using Dirichlet.Numerics;
using UnityEngine;
using UnityEngine.Serialization;

public class BlackContext : MonoBehaviour, IBlackContext
{
    public static IBlackContext instance;

    [SerializeField]
    ScUInt128 freeGem;

    [SerializeField]
    ScInt lastClearedStageId;

    [SerializeField]
    ScUInt128 paidGem;

    [SerializeField]
    ScUInt128 pendingFreeGem;

    [SerializeField]
    ScUInt128 pendingGold;

    [SerializeField]
    PlatformLocalNotification platformLocalNotification;

    [SerializeField]
    MuseumImage museumImage;

    [SerializeField]
    ScUInt128 gold;
    
    [FormerlySerializedAs("stageDetail")]
    [SerializeField]
    StageDetailPopup stageDetailPopup;

    [SerializeField]
    GameObject achievementNewImage;
    
    [SerializeField]
    float stageLockRemainTime;

    List<int> clearedDebrisIndexList;
    

    bool comboAdminMode = false;

    public bool CheatMode { get; set; }
    public bool WaiveBan { get; set; }

    public UInt128 Gold
    {
        get => gold;
        private set
        {
            gold = value;
            OnGoldChanged?.Invoke();
        }
    }

    public UInt128 PendingGold
    {
        get => pendingGold;
        private set => pendingGold = value;
    }

    public bool ComboAdminMode
    {
        get => comboAdminMode;
        set => comboAdminMode = value;
    }

    public void RefreshGoldText()
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

    public event NotifyGoldChange OnGoldChanged;
    public void SetDebrisState(List<int> inClearedDebrisIndexList)
    {
        if (museumImage != null)
        {
            if (inClearedDebrisIndexList != null)
            {
                museumImage.SetDebrisState(inClearedDebrisIndexList);
            }
        }
        else
        {
            clearedDebrisIndexList = inClearedDebrisIndexList;
        }
    }

    public List<int> GetDebrisState()
    {
        return museumImage != null ? museumImage.GetDebrisState() : clearedDebrisIndexList;
    }

    public void SetStageLockRemainTime(float inStageLockRemainTime)
    {
        stageLockRemainTime = inStageLockRemainTime;
        
        if (stageDetailPopup != null)
        {
            stageDetailPopup.StageLockDetailTime = inStageLockRemainTime;
        }
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
                if (paidGem.ToUInt128() < exceeded) throw new ArgumentOutOfRangeException();

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

    public void AddPendingGold(UInt128 delta)
    {
        if (delta == 0) {
            // Do nothing
        } else if (delta > 0) {
            PendingGold += delta;
        } else {
            Debug.LogError("AddPendingGold with negative value");
        }
    }

    public void ApplyPendingGold()
    {
        ConDebug.Log($"ApplyPendingGold: {PendingGold}");
        AddGoldSafe(PendingGold);
        BlackLogManager.Add(BlackLogEntry.Type.GoldAddPending, 0,
            PendingGold < long.MaxValue ? (long) PendingGold : long.MaxValue);
        PendingGold = 0;
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

    public ScInt StageCombo { get; set; } = 0;

    public GameObject AchievementNewImage => achievementNewImage;
    public AchievementRecord1 AchievementGathered { get; set; }
    public AchievementRecord1 AchievementRedeemed { get; set; }
    public ScLong LastDailyRewardRedeemedIndex { get; set; } = 0;
    public ScLong LastDailyRewardRedeemedTicks { get; set; } = DateTime.MinValue.Ticks;
    public ScInt UserPseudoId { get; set; }
    public ScInt LastConsumedServiceIndex { get; set; }
    public bool SaveFileLoaded { get; set; }
    public NoticeData NoticeData { get; set; }
    public bool LoadedAtLeastOnce { get; set; }
    public List<ScInt> WhacACatStageClearLevelList { get; set; }
    public HashSet<string> AlreadyUnlockedPlatformAchievementSet { get; set; }
    public GameObject UserEmergency { get; } = null;
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

    public void SetGold(UInt128 v)
    {
        Gold = v;
    }

    public void SetPendingGold(UInt128 v)
    {
        pendingGold = v;
    }

    public void AddGoldSafe(UInt128 v)
    {
        try
        {
            Gold += v;
        }
        catch (OverflowException)
        {
            Gold = UInt128.MaxValue;
        }
    }

    public void SubtractGold(UInt128 v)
    {
        if (Gold >= v)
            Gold -= v;
        else
            throw new ArgumentOutOfRangeException();
    }

    public Canvas[] CriticalErrorHiddenCanvasList { get; } = null;
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
        SaveLoadManager.Save(this, ConfigPopup.instance, Sound.instance, Data.instance, null);
    }

    void OnApplicationPause(bool pause)
    {
        ConDebug.Log($"{nameof(BlackContext)}.{nameof(OnApplicationPause)}({pause})");

        // 개발 중일 때 종종 이 조건에 걸린다. (코드 고치고 유니티로 돌아온 뒤 플레이 시작하는 경우)
        // 그냥 처리하지 말자. 어차피 처리 중에 오류 난다.
        if (Application.isEditor && Data.dataSet == null)
        {
            return;
        }
        
        if (pause)
        {
            // 백그라운드 상태가 되기 시작할 때 호출된다.
            SaveLoadManager.Save(this, ConfigPopup.instance, Sound.instance, Data.instance, null);

            PlatformLocalNotification.RegisterAllRepeatingNotifications();

            // 게임이 제대로 시작한 이후부터만 백그라운드 처리 보상이 작동해도 된다.
            if (LoadedAtLeastOnce) BackgroundTimeCompensator.instance.BeginBackgroundState(this);
        }
        else
        {
            // 백그라운드 상태가 끝나고 호출된다.
            // 게임이 최초로 실행되는 단계에서도 한번 호출되는 것 같다.
            PlatformLocalNotification.RemoveAllRepeatingNotifications();

            // 게임이 제대로 시작한 이후부터만 백그라운드 처리 보상이 작동해도 된다.
            if (LoadedAtLeastOnce) BackgroundTimeCompensator.instance.EndBackgroundState(this);
        }
    }
}