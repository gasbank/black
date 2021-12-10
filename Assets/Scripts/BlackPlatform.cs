using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ConditionalDebug;
using Dirichlet.Numerics;
using UnityEngine;
using RemoteSaveDictionary = System.Collections.Generic.Dictionary<string, byte[]>;

public class BlackPlatform : MonoBehaviour, IPlatformSaveUtil, IPlatformText, IPlatformConfig {
    public static BlackPlatform instance;

    [SerializeField]
    PlatformInterface platformInterface;

    [SerializeField]
    SaveLoadManager saveLoadManager;

    static readonly string ACCOUNT_LEVEL_KEY = "__accountLevel";
    static readonly string ACCOUNT_LEVEL_EXP_KEY = "__accountLevelExp";
    static readonly string ACCOUNT_GEM_KEY = "__accountGem";
    static readonly string ACCOUNT_RICE_RATE_KEY = "__accountRiceRate";
    static readonly string SAVE_DATE_KEY = "__saveDate";

    public static string GetLoadOverwriteConfirmMessage(CloudMetadata cloudMetadata) {
        return TextHelper.GetText("platform_load_confirm_popup");
    }

    public static string GetSaveOverwriteConfirmMessage(CloudMetadata cloudMetadata) {
        return TextHelper.GetText("platform_save_confirm_popup");
    }

    public static bool IsLoadRollback(CloudMetadata cloudMetadata) {
        return cloudMetadata.level < ResourceManager.instance.accountLevel
               || cloudMetadata.levelExp < ResourceManager.instance.accountLevelExp
               || cloudMetadata.gem < ResourceManager.instance.accountGem
               || cloudMetadata.riceRate < ResourceManager.instance.accountRiceRate;
    }

    public static bool IsSaveRollback(CloudMetadata cloudMetadata) {
        return cloudMetadata.level > ResourceManager.instance.accountLevel
               || cloudMetadata.levelExp > ResourceManager.instance.accountLevelExp
               || cloudMetadata.gem > ResourceManager.instance.accountGem
               || cloudMetadata.riceRate > ResourceManager.instance.accountRiceRate;
    }

    public static void LoadSplashScene() {
        Splash.LoadSplashScene();
    }

    public byte[] SerializeSaveData() {
        var dict = new RemoteSaveDictionary();
        // 직전 단계에서 PlatformInterface.instance.saveLoadManager.SaveFileName에다가 썼다.
        // 쓰면서 save slot을 1 증가시켰으니까, 직전에 쓴 파일을 읽어오고 싶으면
        // PlatformInterface.instance.saveLoadManager.LoadFileName을 써야 한다.
        // 저장하는 키는 'save.dat'로 고정이다. (하위호환성)
        var localSaveFileName = SaveLoadManager.GetSaveLoadFileNameOnly(0);
        ConDebug.Log($"Saving '{SaveLoadManager.LoadFileName}' to a dict key '{localSaveFileName}'");
        dict[localSaveFileName] = File.ReadAllBytes(SaveLoadManager.LoadFileName);
        dict[ACCOUNT_LEVEL_KEY] = BitConverter.GetBytes(ResourceManager.instance.accountLevel);
        dict[ACCOUNT_LEVEL_EXP_KEY] = BitConverter.GetBytes(ResourceManager.instance.accountLevelExp);
        dict[ACCOUNT_GEM_KEY] = UInt128BigInteger.ToBigInteger(ResourceManager.instance.accountGem).ToByteArray();
        dict[ACCOUNT_RICE_RATE_KEY] = UInt128BigInteger.ToBigInteger(ResourceManager.instance.accountRiceRate).ToByteArray();
        dict[SAVE_DATE_KEY] = BitConverter.GetBytes(DateTime.Now.Ticks);
        var binFormatter = new BinaryFormatter();
        var memStream = new MemoryStream();
        binFormatter.Serialize(memStream, dict);
        return memStream.ToArray();
    }

    public static void OpenTwoButtonPopup_Update(string msg, Action onBtn1, Action onBtn2) {
        ConfirmPopup.instance.OpenTwoButtonPopup(msg, onBtn1, onBtn2, "\\확인".Localized(), "\\확인".Localized(), "Update");
    }

    public static PlatformNotificationRequest GetPlatformNotificationRequest() {
        if (BlackSpawner.instance == null) {
            Debug.LogError($"RegisterAllRepeatingNotifications(): {nameof(BlackSpawner)}.{nameof(BlackSpawner.instance)} is null. Abort.");
            return null;
        }

        if (Data.dataSet == null) {
            Debug.LogError($"RegisterAllRepeatingNotifications(): {nameof(Data)}.{nameof(Data.dataSet)} is null. Abort.");
            return null;
        }

        if (BlackSpawner.instance.LastDailyRewardRedeemedIndex < Data.dataSet.DailyRewardData.Count) {
            var data = Data.dataSet.DailyRewardData[(int)BlackSpawner.instance.LastDailyRewardRedeemedIndex.ToLong()];
            var title = "\\{0}일차 이벤트".Localized(BlackSpawner.instance.LastDailyRewardRedeemedIndex + 1);
            // iOS는 이모지 지원이 된다!
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                title = $"{title}";
            }

            var desc = data.notificationDesc.Localized(data.amount.ToInt());
            var largeIconIndex = Mathf.Max(0, BlackSpawner.instance.MahjongLastClearedStageId - 1);

            var currentDate = DateTime.Now;
            var localZone = TimeZoneInfo.Local;
            var currentOffset = localZone.GetUtcOffset(currentDate);
            var localHours = currentOffset.Hours;
            if (localHours < 0) {
                localHours += 24;
            }

            PlatformNotificationRequest request = new PlatformNotificationRequest {
                title = title,
                body = desc,
                largeIcon = string.Format("su_00_{0:D3}", largeIconIndex),
                localHours = localHours
            };

            ConDebug.Log($"RegisterAllRepeatingNotifications UTC Offset Hour: {localHours}");

            return request;
        }

        return null;
    }

    public string Str_InternetRequiredForAds => "\\광고를 보기 위해서는 인터넷 연결이 필요합니다.".Localized();
    public string Str_AdsAborted => "\\광고 시청 도중 중단됐습니다.".Localized();
    public string Str_UnityAdsError => "\\$Unity Ads 광고 볼 수 없는 사유 설명$".Localized();
    public string Str_AdMobError => "\\$AdMob 광고 볼 수 없는 사유 설명$".Localized();

    public string Str_FacebookAdsError =>
        "\\$Unity Ads 광고 볼 수 없는 사유 설명$".Localized(); // Facebook 광고 에러 메시지는 따로 안만들었다?

    public string Str_SearchingAds => "\\광고를 찾고 있습니다...".Localized();
    public string LoginErrorTitle => "\\iCloud 로그인 필요".Localized();
    public string GetNetworkTimeQueryProgressText(int oneBasedIndex, int totalCount) {
        return Data.dataSet != null ? "\\오늘 날짜 확인중...\\n(서버 {0}개 중 {1}개 확인)\\n\\n잠시만 기다려 주십시오.".Localized(totalCount, oneBasedIndex) : "...";
    }

    public string LoginErrorMessage =>
        "\\홈 화면 -> 설정 -> (내 계정) -> iCloud에 로그인 해 주세요. 그 다음 Game Center, iCloud Drive, 컬러뮤지엄 항목을 켜고 다시 시도 해 주세요."
            .Localized();

    public string ConfirmMessage => "\\확인".Localized();

    public class CloudMetadata {
        public static readonly CloudMetadata Invalid = new CloudMetadata
            {level = 0, levelExp = 0, gem = 0, riceRate = 0, saveDate = 0};

        public int level;
        public int levelExp;
        public UInt128 gem;
        public UInt128 riceRate;
        public long saveDate;
    }

    public void SaveBeforeCloudSave() {
        saveLoadManager.Save(BlackSpawner.instance, ConfigPopup.instance, Sound.instance, Data.instance);
    }

    public RemoteSaveDictionary DeserializeSaveData(byte[] bytes) {
        try {
            if (bytes == null) return null;

            var memStream = new MemoryStream();
            var binFormatter = new BinaryFormatter();
            memStream.Write(bytes, 0, bytes.Length);
            memStream.Position = 0;
            return binFormatter.Deserialize(memStream) as RemoteSaveDictionary;
        } catch (SerializationException e) {
            Debug.LogException(e);
            return null;
        }
    }

    public CloudMetadata GetCloudMetadataFromBytes(byte[] byteArr) {
        var remoteSaveDict = DeserializeSaveData(byteArr);

        if (remoteSaveDict == null) return null;

        return new CloudMetadata {
            level = GetInt32FromRemoteSaveDict(remoteSaveDict,
                ACCOUNT_LEVEL_KEY),
            levelExp = GetInt32FromRemoteSaveDict(remoteSaveDict,
                ACCOUNT_LEVEL_EXP_KEY),
            gem = UInt128BigInteger.FromBigInteger(GetBigIntegerFromRemoteSaveDict(remoteSaveDict,
                ACCOUNT_GEM_KEY)),
            riceRate = UInt128BigInteger.FromBigInteger(GetBigIntegerFromRemoteSaveDict(remoteSaveDict,
                ACCOUNT_RICE_RATE_KEY)),
            saveDate = GetInt64FromRemoteSaveDict(remoteSaveDict,
                SAVE_DATE_KEY),
        };
    }

    public void LoadDataAndLoadSplashScene(RemoteSaveDictionary dict) {
        // 모든 저장 파일을 지운다.
        SaveLoadManager.DeleteAllSaveFiles();
        // 그 다음 쓴다.
        foreach (var fileName in dict) {
            var filePath = Path.Combine(Application.persistentDataPath, fileName.Key);
            ConDebug.Log(
                $"LoadDataAndLoadSplashScene: gd key = {fileName.Key}, length = {fileName.Value.Length:n0}, writing to = {filePath}");
            File.WriteAllBytes(filePath, fileName.Value);
        }

        ConDebug.Log("LoadCloudDataAndSave");
        PlatformInterface.instance.logManager.Add(PlatformInterface.instance.logEntryType.GameCloudLoadEnd, 0, 0);
        Splash.LoadSplashScene();
    }


    int GetInt32FromRemoteSaveDict(RemoteSaveDictionary remoteSaveDict, string key) {
        if (remoteSaveDict.TryGetValue(key, out byte[] _)) {
            return BitConverter.ToInt32(remoteSaveDict[key], 0);
        } else {
            return -1;
        }
    }

    long GetInt64FromRemoteSaveDict(RemoteSaveDictionary remoteSaveDict, string key) {
        if (remoteSaveDict.TryGetValue(key, out byte[] _)) {
            return BitConverter.ToInt64(remoteSaveDict[key], 0);
        } else {
            return -1;
        }
    }

    System.Numerics.BigInteger GetBigIntegerFromRemoteSaveDict(RemoteSaveDictionary remoteSaveDict,
        string key) {
        if (remoteSaveDict.TryGetValue(key, out byte[] _)) {
            return new System.Numerics.BigInteger(remoteSaveDict[key]);
        } else {
            return -1;
        }
    }

    public string GetEditorSaveResultText(byte[] savedData, RemoteSaveDictionary remoteSaveDict, string path) {
        return
            $"세이브 완료 - age: {TimeChecker.instance.GetLastSavedTimeTotalSeconds()} sec, size: {savedData.Length} bytes, accountLevel = {GetInt32FromRemoteSaveDict(remoteSaveDict, ACCOUNT_LEVEL_KEY)}, accountLevelExp = {GetInt32FromRemoteSaveDict(remoteSaveDict, ACCOUNT_LEVEL_EXP_KEY)}, accountGem = {GetBigIntegerFromRemoteSaveDict(remoteSaveDict, ACCOUNT_GEM_KEY)}, savedDate = {GetInt64FromRemoteSaveDict(remoteSaveDict, SAVE_DATE_KEY)}, path = {path}";
    }

    public TimeSpan GetPlayed() =>
        TimeSpan.FromSeconds(BlackSpawner.instance
            .PlayTimeSec); // System.TimeSpan.Zero;//NetworkTime.GetNetworkTime() - NetworkTime.BaseDateTime;

    public string GetDesc(byte[] bytes) {
        var remoteSaveDict = DeserializeSaveData(bytes);
        var accountLevel = GetInt32FromRemoteSaveDict(remoteSaveDict, ACCOUNT_LEVEL_KEY);
        var accountLevelExp = GetInt32FromRemoteSaveDict(remoteSaveDict, ACCOUNT_LEVEL_EXP_KEY);
        var accountGem = GetBigIntegerFromRemoteSaveDict(remoteSaveDict, ACCOUNT_GEM_KEY);
        return $"Level {accountLevel} / Exp {accountLevelExp} / Gem {accountGem}";
    }

    public void DebugPrintCloudMetadata(byte[] bytes) {
        var cloudMetadata = GetCloudMetadataFromBytes(bytes);
        if (cloudMetadata != null) {
            ConDebug.LogFormat("prevAccountLevel = {0}", cloudMetadata.level);
            ConDebug.LogFormat("prevAccountLevelExp = {0}", cloudMetadata.levelExp);
            ConDebug.LogFormat("prevAccountGem = {0}", cloudMetadata.gem);
            ConDebug.LogFormat("prevAccountRiceRate = {0}", cloudMetadata.riceRate);
            ConDebug.LogFormat("prevSaveDate = {0}", cloudMetadata.saveDate);
        } else {
            ConDebug.LogFormat("Cloud metadata is null.");
        }
    }

    public bool IsValidCloudMetadata(byte[] bytes) {
        var cloudMetadata = GetCloudMetadataFromBytes(bytes);
        return cloudMetadata != null && cloudMetadata.level >= 0 && cloudMetadata.levelExp >= 0 &&
               cloudMetadata.saveDate >= 0;
    }

    public string GetAdMobAppId() {
#if UNITY_ANDROID
        string appId = "unexpected_platform";
#elif UNITY_IOS
        string appId = "unexpected_platform";
#else
        string appId = "unexpected_platform";
#endif
        return appId;
    }

    public string GetAdMobRewardVideoAdUnitId() {
#if BLACK_ADMIN
#if UNITY_ANDROID
        string adUnitId = "unexpected_platform";
#elif UNITY_IOS
        string adUnitId = "unexpected_platform";
#else
        string adUnitId = "unexpected_platform";
#endif
#else
#if UNITY_ANDROID
        string adUnitId = "unexpected_platform";
#elif UNITY_IOS
        string adUnitId = "unexpected_platform";
#else
        string adUnitId = "unexpected_platform";
#endif
#endif
        return adUnitId;
    }

    public string NotificationManagerFullClassName => "top.plusalpha.notification.NotificationManager";

    // 버그 신고 기능과 스크린샷 기능은 기능상은 다르지만, 라이브러리를 따로 추가하지
    // 않고 구현했으므로 같은 이름을 쓴다.
    public string ScreenshotAndReportFullClassName => "top.plusalpha.screenshot.Screenshot";

    public string GetFacebookAdsPlacementId() {
#if UNITY_ANDROID
        var PLACEMENT_ID = "NOTSUPPORTED";
#elif UNITY_IOS
        var PLACEMENT_ID = "NOTSUPPORTED";
#else
        var PLACEMENT_ID = "NOTSUPPORTED";
#endif
        return PLACEMENT_ID;
    }

    // ReSharper disable once RedundantExplicitArrayCreation
    public string[] FacebookAdsTestDeviceIdList => new string[] {
#if BLACK_DEBUG
#endif
    };

    public string GetUserReviewUrl() {
        switch (Application.platform) {
            case RuntimePlatform.Android:
                return "market://details?id=top.plusalpha.black";
            case RuntimePlatform.IPhonePlayer:
                return "itms-apps://itunes.apple.com/app/id9999999999";
            default:
                return "https://daum.net";
        }
    }

    // Application.platform으로 체크하면 에디터 환경에서 처리가 지저분해지니 이렇게 하자.
#if UNITY_ANDROID
    public string UnityAdsGameId => "unsupported platform";
    public bool UnityAdsUseAds => true;
#elif UNITY_IOS
    public string UnityAdsGameId => "unsupported platform";
    public bool UnityAdsUseAds => true;
#else
    public string UnityAdsGameId => "unsupported platform";
    public bool UnityAdsUseAds => false;
#endif

    // iOS에서는 지금 UnityAds가 테스트 모드로 작동 안한다. 컴파일러 프래그 조건이 이모양!
#if BLACK_ADMIN && UNITY_ANDROID
    public bool UnityAdsUseTestMode => true;
#else
    public bool UnityAdsUseTestMode => false;
#endif
}