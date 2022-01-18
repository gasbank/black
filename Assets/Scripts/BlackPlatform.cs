using System;
using System.IO;
using System.Numerics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ConditionalDebug;
using Dirichlet.Numerics;
using UnityEngine;
using RemoteSaveDictionary = System.Collections.Generic.Dictionary<string, byte[]>;

public class BlackPlatform : MonoBehaviour, IPlatformSaveUtil, IPlatformText, IPlatformConfig
{
    public static BlackPlatform instance;

    [SerializeField]
    PlatformInterface platformInterface;

    [SerializeField]
    SaveLoadManager saveLoadManager;

    static readonly string ACCOUNT_LEVEL_KEY = "__accountLevel";
    static readonly string ACCOUNT_LEVEL_EXP_KEY = "__accountLevelExp";
    static readonly string ACCOUNT_GEM_KEY = "__accountGem";
    static readonly string ACCOUNT_GOLD_RATE_KEY = "__accountGoldRate";
    static readonly string SAVE_DATE_KEY = "__saveDate";

    public static string GetLoadOverwriteConfirmMessage(CloudMetadata cloudMetadata)
    {
        return TextHelper.GetText("platform_load_confirm_popup");
    }

    public static string GetSaveOverwriteConfirmMessage(CloudMetadata cloudMetadata)
    {
        return TextHelper.GetText("platform_save_confirm_popup");
    }

    public static bool IsLoadRollback(CloudMetadata cloudMetadata)
    {
        return cloudMetadata.level < ResourceManager.instance.accountLevel
               || cloudMetadata.levelExp < ResourceManager.instance.accountLevelExp
               || cloudMetadata.gem < ResourceManager.instance.accountGem
               || cloudMetadata.goldRate < ResourceManager.instance.accountGoldRate;
    }

    public static bool IsSaveRollback(CloudMetadata cloudMetadata)
    {
        return cloudMetadata.level > ResourceManager.instance.accountLevel
               || cloudMetadata.levelExp > ResourceManager.instance.accountLevelExp
               || cloudMetadata.gem > ResourceManager.instance.accountGem
               || cloudMetadata.goldRate > ResourceManager.instance.accountGoldRate;
    }

    public static void LoadSplashScene()
    {
        Splash.LoadSplashScene();
    }

    public byte[] SerializeSaveData()
    {
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
        dict[ACCOUNT_GOLD_RATE_KEY] =
            UInt128BigInteger.ToBigInteger(ResourceManager.instance.accountGoldRate).ToByteArray();
        dict[SAVE_DATE_KEY] = BitConverter.GetBytes(DateTime.Now.Ticks);
        var binFormatter = new BinaryFormatter();
        var memStream = new MemoryStream();
        binFormatter.Serialize(memStream, dict);
        return memStream.ToArray();
    }

    public static void OpenTwoButtonPopup_Update(string msg, Action onBtn1, Action onBtn2)
    {
        ConfirmPopup.instance.OpenTwoButtonPopup(msg, onBtn1, onBtn2, "\\확인".Localized(), "\\확인".Localized(), "Update");
    }

    public static PlatformNotificationRequest GetPlatformNotificationRequest()
    {
        if (BlackContext.instance == null)
        {
            Debug.LogError(
                $"RegisterAllRepeatingNotifications(): {nameof(BlackContext)}.{nameof(BlackContext.instance)} is null. Abort.");
            return null;
        }

        if (Data.dataSet == null)
        {
            Debug.LogError(
                $"RegisterAllRepeatingNotifications(): {nameof(Data)}.{nameof(Data.dataSet)} is null. Abort.");
            return null;
        }

        if (BlackContext.instance.LastDailyRewardRedeemedIndex < Data.dataSet.DailyRewardData.Count)
        {
            var data = Data.dataSet.DailyRewardData[(int) BlackContext.instance.LastDailyRewardRedeemedIndex.ToLong()];
            var title = "\\{0}일차 이벤트".Localized(BlackContext.instance.LastDailyRewardRedeemedIndex + 1);
            // iOS는 이모지 지원이 된다!
            if (Application.platform == RuntimePlatform.IPhonePlayer) title = $"{title}";

            var desc = data.notificationDesc.Localized(data.amount.ToInt());
            var largeIconIndex = Mathf.Max(0, BlackContext.instance.LastClearedStageId - 1);

            var currentDate = DateTime.Now;
            var localZone = TimeZoneInfo.Local;
            var currentOffset = localZone.GetUtcOffset(currentDate);
            var localHours = currentOffset.Hours;
            if (localHours < 0) localHours += 24;

            var request = new PlatformNotificationRequest
            {
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

    public string GetNetworkTimeQueryProgressText(int oneBasedIndex, int totalCount)
    {
        return Data.dataSet != null
            ? "\\오늘 날짜 확인중...\\n(서버 {0}개 중 {1}개 확인)\\n\\n잠시만 기다려 주십시오.".Localized(totalCount, oneBasedIndex)
            : "...";
    }

    public string LoginErrorMessage =>
        "\\홈 화면 -> 설정 -> (내 계정) -> iCloud에 로그인 해 주세요. 그 다음 Game Center, iCloud Drive, 컬러뮤지엄 항목을 켜고 다시 시도 해 주세요."
            .Localized();

    public string ConfirmMessage => "\\확인".Localized();

    public class CloudMetadata
    {
        public static readonly CloudMetadata Invalid = new CloudMetadata
            {level = 0, levelExp = 0, gem = 0, goldRate = 0, saveDate = 0};

        public UInt128 gem;

        public int level;
        public int levelExp;
        public UInt128 goldRate;
        public long saveDate;
    }

    public void SaveBeforeCloudSave()
    {
        SaveLoadManager.Save(BlackContext.instance, ConfigPopup.instance, Sound.instance, Data.instance, null);
    }

    public RemoteSaveDictionary DeserializeSaveData(byte[] bytes)
    {
        try
        {
            if (bytes == null) return null;

            var memStream = new MemoryStream();
            var binFormatter = new BinaryFormatter();
            memStream.Write(bytes, 0, bytes.Length);
            memStream.Position = 0;
            return binFormatter.Deserialize(memStream) as RemoteSaveDictionary;
        }
        catch (SerializationException e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    public CloudMetadata GetCloudMetadataFromBytes(byte[] byteArr)
    {
        var remoteSaveDict = DeserializeSaveData(byteArr);

        if (remoteSaveDict == null) return null;

        return new CloudMetadata
        {
            level = GetInt32FromRemoteSaveDict(remoteSaveDict,
                ACCOUNT_LEVEL_KEY),
            levelExp = GetInt32FromRemoteSaveDict(remoteSaveDict,
                ACCOUNT_LEVEL_EXP_KEY),
            gem = UInt128BigInteger.FromBigInteger(GetBigIntegerFromRemoteSaveDict(remoteSaveDict,
                ACCOUNT_GEM_KEY)),
            goldRate = UInt128BigInteger.FromBigInteger(GetBigIntegerFromRemoteSaveDict(remoteSaveDict,
                ACCOUNT_GOLD_RATE_KEY)),
            saveDate = GetInt64FromRemoteSaveDict(remoteSaveDict,
                SAVE_DATE_KEY)
        };
    }

    public void LoadDataAndLoadSplashScene(RemoteSaveDictionary dict)
    {
        // 모든 저장 파일을 지운다.
        SaveLoadManager.DeleteAllSaveFiles();
        // 그 다음 쓴다.
        foreach (var fileName in dict)
        {
            var filePath = Path.Combine(Application.persistentDataPath, fileName.Key);
            ConDebug.Log(
                $"LoadDataAndLoadSplashScene: gd key = {fileName.Key}, length = {fileName.Value.Length:n0}, writing to = {filePath}");
            File.WriteAllBytes(filePath, fileName.Value);
        }

        ConDebug.Log("LoadCloudDataAndSave");
        PlatformInterface.instance.logManager.Add(PlatformInterface.instance.logEntryType.GameCloudLoadEnd, 0, 0);
        Splash.LoadSplashScene();
    }


    int GetInt32FromRemoteSaveDict(RemoteSaveDictionary remoteSaveDict, string key)
    {
        if (remoteSaveDict.TryGetValue(key, out _))
            return BitConverter.ToInt32(remoteSaveDict[key], 0);
        return -1;
    }

    long GetInt64FromRemoteSaveDict(RemoteSaveDictionary remoteSaveDict, string key)
    {
        if (remoteSaveDict.TryGetValue(key, out _))
            return BitConverter.ToInt64(remoteSaveDict[key], 0);
        return -1;
    }

    BigInteger GetBigIntegerFromRemoteSaveDict(RemoteSaveDictionary remoteSaveDict,
        string key)
    {
        if (remoteSaveDict.TryGetValue(key, out _))
            return new BigInteger(remoteSaveDict[key]);
        return -1;
    }

    public string GetEditorSaveResultText(byte[] savedData, RemoteSaveDictionary remoteSaveDict, string path)
    {
        return
            $"세이브 완료 - age: {TimeChecker.instance.GetLastSavedTimeTotalSeconds()} sec, size: {savedData.Length} bytes, accountLevel = {GetInt32FromRemoteSaveDict(remoteSaveDict, ACCOUNT_LEVEL_KEY)}, accountLevelExp = {GetInt32FromRemoteSaveDict(remoteSaveDict, ACCOUNT_LEVEL_EXP_KEY)}, accountGem = {GetBigIntegerFromRemoteSaveDict(remoteSaveDict, ACCOUNT_GEM_KEY)}, savedDate = {GetInt64FromRemoteSaveDict(remoteSaveDict, SAVE_DATE_KEY)}, path = {path}";
    }

    public TimeSpan GetPlayed()
    {
        return TimeSpan.FromSeconds(BlackContext.instance
            .PlayTimeSec);
    }

    public string GetDesc(byte[] bytes)
    {
        var remoteSaveDict = DeserializeSaveData(bytes);
        var accountLevel = GetInt32FromRemoteSaveDict(remoteSaveDict, ACCOUNT_LEVEL_KEY);
        var accountLevelExp = GetInt32FromRemoteSaveDict(remoteSaveDict, ACCOUNT_LEVEL_EXP_KEY);
        var accountGem = GetBigIntegerFromRemoteSaveDict(remoteSaveDict, ACCOUNT_GEM_KEY);
        return $"Level {accountLevel} / Exp {accountLevelExp} / Gem {accountGem}";
    }

    public void DebugPrintCloudMetadata(byte[] bytes)
    {
        var cloudMetadata = GetCloudMetadataFromBytes(bytes);
        if (cloudMetadata != null)
        {
            ConDebug.LogFormat("prevAccountLevel = {0}", cloudMetadata.level);
            ConDebug.LogFormat("prevAccountLevelExp = {0}", cloudMetadata.levelExp);
            ConDebug.LogFormat("prevAccountGem = {0}", cloudMetadata.gem);
            ConDebug.LogFormat("prevAccountGoldRate = {0}", cloudMetadata.goldRate);
            ConDebug.LogFormat("prevSaveDate = {0}", cloudMetadata.saveDate);
        }
        else
        {
            ConDebug.LogFormat("Cloud metadata is null.");
        }
    }

    public bool IsValidCloudMetadata(byte[] bytes)
    {
        var cloudMetadata = GetCloudMetadataFromBytes(bytes);
        return cloudMetadata != null && cloudMetadata.level >= 0 && cloudMetadata.levelExp >= 0 &&
               cloudMetadata.saveDate >= 0;
    }

    public string GetAdMobAppId()
    {
#if UNITY_ANDROID
        var appId = "unexpected_platform";
#elif UNITY_IOS
        string appId = "unexpected_platform";
#else
        string appId = "unexpected_platform";
#endif
        return appId;
    }

    // Ad Unit ID는 테스트용과 실서비스 용이 있다.
    // 그런데 좀 복잡하다.
    // 
    // 테스트기기 등록된 경우에는 실서비스 용을 써야 테스트 광고가 나온다.
    // (1) 실제 유저 기기인 경우에는 실서비스 용을 쓰면 된다.
    // (2) 개발 기기인 경우에는 두 가지로 나뉜다.
    //     (A) 테스트 기기 등록되었다면 실서비스용 Ad Unit ID를 써야된다.
    //     (B) 테스트 기기 등록되어있지 않다면 테스트용 Ad Unit ID를 써야한다.
    //
    // 테스트 기기 등록을 위한 ID는 앱 재설치로도 바뀔 수 있으니, 자주 바뀐다고 봐야 한다.
    // (사실 언제 바뀌는지 정확히 모르겠다.)
    // 실서비스용 & 실사용자 대상으로 작동에는 문제 없지만 개발 과정 중엔
    // 어드민 커맨드로 Ad Unit ID를 실서비스용과 테스트용으로 토글링할 수 있도록 해서 쓰자.
    public string GetAdMobRewardVideoAdUnitId()
    {
        var adUnitId = "unexpected_platform";

        if (Admin.IsAdUnitIdModeTest)
        {
            adUnitId = Application.platform switch
            {
                // https://developers.google.com/admob/unity/test-ads#android
                RuntimePlatform.Android => "ca-app-pub-3940256099942544/5224354917",
                
                // https://developers.google.com/admob/unity/test-ads#ios
                RuntimePlatform.IPhonePlayer => "ca-app-pub-3940256099942544/1712485313",
                
                _ => adUnitId
            };
        }
        else
        {
            adUnitId = Application.platform switch
            {
                RuntimePlatform.Android => "ca-app-pub-5072035175916776/7928389116",
                RuntimePlatform.IPhonePlayer => "ca-app-pub-5072035175916776/4851266226",
                _ => adUnitId
            };
        }

        return adUnitId;
    }

    public string GetFacebookAdsPlacementId()
    {
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
    public string[] FacebookAdsTestDeviceIdList => new string[]
    {
#if BLACK_DEBUG
#endif
    };

    public string GetUserReviewUrl()
    {
        switch (Application.platform)
        {
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