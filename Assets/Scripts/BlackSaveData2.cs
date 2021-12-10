using System.Collections.Generic;

[System.Serializable]
public class BlackSaveData2
{
    public ScInt version = 0;
    public ScInt lastClearedStageId;
    public ScUInt128 riceScUInt128 = 0;
    public ScUInt128 pendingRiceScUInt128 = 0;
    public ScUInt128 freeGemScUInt128 = 0;
    public ScUInt128 pendingFreeGemScUInt128 = 0;
    public ScUInt128 paidGemScUInt128 = 0;
    public List<ScString> stashedRewardJsonList;
    public List<ScLong> lastDailyRewardRedeemedTicksList;
    public ScInt noAdsCode;
    public ScLong lastDailyRewardRedeemedIndex = 0;
    public ScLong lastDailyRewardRedeemedTicks = 0;

    // Config
    public bool muteBgmAudioSource;
    public bool muteSfxAudioSource;
    public float bgmAudioVolume = 1.0f;
    public float sfxAudioVolume = 1.0f;
    public bool notchSupport;
    public bool bottomNotchSupport;
    public bool performanceMode;
    public bool alwaysOn;
    public bool bigScreen;
    public ScInt userPseudoId = 0;
    public ScInt lastConsumedServiceIndex = 0;
    public BlackLanguageCode languageCode = BlackLanguageCode.Ko;
    public NoticeData noticeData;

    // localUser.id -> local user data
    public Dictionary<string, LocalUserData> localUserDict;

    public Dictionary<ScString, ScInt> purchasedProductDict;
    public Dictionary<ScString, List<ScString>> purchasedProductReceipts;
    public HashSet<ScString> verifiedProductReceipts;
    public bool cheatMode;
    public bool waiveBan;
}