using System.Collections.Generic;
using MessagePack;

[System.Serializable]
[MessagePackObject]
public class BlackSaveData
{
    [Key(0)]
    public ScInt version = 0;

    [Key(1)]
    public ScInt lastClearedStageId;

    [Key(2)]
    public ScUInt128 riceScUInt128 = 0;

    [Key(3)]
    public ScUInt128 pendingRiceScUInt128 = 0;

    [Key(4)]
    public ScUInt128 freeGemScUInt128 = 0;

    [Key(5)]
    public ScUInt128 pendingFreeGemScUInt128 = 0;

    [Key(6)]
    public ScUInt128 paidGemScUInt128 = 0;

    [Key(7)]
    public List<ScString> stashedRewardJsonList;

    [Key(8)]
    public List<ScLong> lastDailyRewardRedeemedTicksList;

    [Key(9)]
    public ScInt noAdsCode;

    [Key(10)]
    public ScLong lastDailyRewardRedeemedIndex = 0;

    [Key(11)]
    public ScLong lastDailyRewardRedeemedTicks = 0;

    // Config
    [Key(12)]
    public bool muteBgmAudioSource;

    [Key(13)]
    public bool muteSfxAudioSource;

    [Key(14)]
    public float bgmAudioVolume = 1.0f;

    [Key(15)]
    public float sfxAudioVolume = 1.0f;

    [Key(16)]
    public bool notchSupport;

    [Key(17)]
    public bool bottomNotchSupport;

    [Key(18)]
    public bool performanceMode;

    [Key(19)]
    public bool alwaysOn;

    [Key(20)]
    public bool bigScreen;

    [Key(21)]
    public ScInt userPseudoId = 0;

    [Key(22)]
    public ScInt lastConsumedServiceIndex = 0;

    [Key(23)]
    public BlackLanguageCode languageCode = BlackLanguageCode.Ko;

    [Key(24)]
    public NoticeData noticeData;

    // localUser.id -> local user data
    [Key(25)]
    public Dictionary<string, LocalUserData> localUserDict;

    [Key(26)]
    public Dictionary<ScString, ScInt> purchasedProductDict;

    [Key(27)]
    public Dictionary<ScString, List<ScString>> purchasedProductReceipts;

    [Key(28)]
    public HashSet<ScString> verifiedProductReceipts;

    [Key(29)]
    public bool cheatMode;

    [Key(30)]
    public bool waiveBan;
}