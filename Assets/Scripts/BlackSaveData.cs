using System;
using System.Collections.Generic;
using MessagePack;

[Serializable]
[MessagePackObject]
public class BlackSaveData
{
    [Key(19)]
    public bool alwaysOn;

    [Key(14)]
    public float bgmAudioVolume = 1.0f;

    [Key(20)]
    public bool bigScreen;

    [Key(17)]
    public bool bottomNotchSupport;

    [Key(29)]
    public bool cheatMode;

    [Key(4)]
    public ScUInt128 freeGemScUInt128 = 0;

    [Key(23)]
    public BlackLanguageCode languageCode = BlackLanguageCode.Ko;

    [Key(1)]
    public ScInt lastClearedStageId;

    [Key(22)]
    public ScInt lastConsumedServiceIndex = 0;

    [Key(10)]
    public ScLong lastDailyRewardRedeemedIndex = 0;

    [Key(11)]
    public ScLong lastDailyRewardRedeemedTicks = 0;

    [Key(8)]
    public List<ScLong> lastDailyRewardRedeemedTicksList;

    // localUser.id -> local user data
    [Key(25)]
    public Dictionary<string, LocalUserData> localUserDict;

    // Config
    [Key(12)]
    public bool muteBgmAudioSource;

    [Key(13)]
    public bool muteSfxAudioSource;

    [Key(9)]
    public ScInt noAdsCode;

    [Key(16)]
    public bool notchSupport;

    [Key(24)]
    public NoticeData noticeData;

    [Key(6)]
    public ScUInt128 paidGemScUInt128 = 0;

    [Key(5)]
    public ScUInt128 pendingFreeGemScUInt128 = 0;

    [Key(3)]
    public ScUInt128 pendingGoldScUInt128 = 0;

    [Key(18)]
    public bool performanceMode;

    [Key(26)]
    public Dictionary<ScString, ScInt> purchasedProductDict;

    [Key(27)]
    public Dictionary<ScString, List<ScString>> purchasedProductReceipts;

    [Key(2)]
    public ScUInt128 goldScUInt128 = 0;

    [Key(15)]
    public float sfxAudioVolume = 1.0f;

    [Key(7)]
    public List<ScString> stashedRewardJsonList;

    [Key(21)]
    public ScInt userPseudoId = 0;

    [Key(28)]
    public HashSet<ScString> verifiedProductReceipts;

    [Key(0)]
    public ScInt version = 0;

    [Key(30)]
    public bool waiveBan;

    [Key(31)]
    public List<int> clearedDebrisIndexList;

    [Key(32)]
    public ScUInt128 maxBlackLevelGathered;

    [Key(33)]
    public ScUInt128 maxBlackLevelRedeemed;

    [Key(34)]
    public ScUInt128 maxColoringComboGathered;

    [Key(35)]
    public ScUInt128 maxColoringComboRedeemed;

    [Key(36)]
    public float stageLockRemainTime;

    [Key(37)]
    StageSaveData wipStageSaveData;

    [Key(38)]
    byte[] wipPngData;
}