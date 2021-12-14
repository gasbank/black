using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System;
using System.Diagnostics.CodeAnalysis;
using ConditionalDebug;
using Dirichlet.Numerics;

public class SaveLoadManager : MonoBehaviour, IPlatformSaveLoadManager
{
    static readonly string localSaveFileName = "save.dat";

    public static SaveLoadManager instance;

    [SerializeField]
    BlackContext blackContext;

    [SerializeField]
    NetworkTime networkTime;

    // 총 maxSaveDataSlot개의 저장 슬롯이 있고, 이를 돌려가며 쓴다.
    public static readonly int maxSaveDataSlot = 9;
    static readonly string saveDataSlotKey = "Save Data Slot";

    public static string SaveFileName
    {
        get { return GetSaveLoadFilePathName(GetSaveSlot() + 1); }
    }

    public static string LoadFileName
    {
        get { return GetSaveLoadFilePathName(GetSaveSlot()); }
    }

    static int PositiveMod(int x, int m)
    {
        return (x % m + m) % m;
    }

    const int LatestVersion = 1;

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum SaveReason
    {
        Quit,
        Pause,
        AutoSave,
        BeforeMahjong,
    }

    static byte[] lastSaveDataArray;

    void Start()
    {
        // 저장 데이터 로드 *** 다른 모든 초기화 이전에 완료되어 있어야 하는 작업 ***
        Load(blackContext);
    }

    static string GetSaveLoadFilePathName(int saveDataSlot)
    {
        return Path.Combine(Application.persistentDataPath, GetSaveLoadFileNameOnly(saveDataSlot));
    }

    public static string GetSaveLoadFileNameOnly(int saveDataSlot)
    {
        saveDataSlot = PositiveMod(saveDataSlot, maxSaveDataSlot);
        // 하위 호환성을 위해 0인 경우 기존 이름을 쓴다.
        return saveDataSlot == 0 ? localSaveFileName : $"save{saveDataSlot}.dat";
    }

    static int GetSaveSlot()
    {
        return PlayerPrefs.GetInt(saveDataSlotKey, 0);
    }

    // 저장 슬롯 증가 (성공적인 저장 후 항상 1씩 증가되어야 함)
    public static void IncreaseSaveDataSlotAndWrite()
    {
        var oldSaveDataSlot = GetSaveSlot();
        var newSaveDataSlot = PositiveMod(oldSaveDataSlot + 1, maxSaveDataSlot);
        ConDebug.Log($"Increase save data slot from {oldSaveDataSlot} to {newSaveDataSlot}...");
        PlayerPrefs.SetInt(saveDataSlotKey, newSaveDataSlot);
        PlayerPrefs.Save();
        ConDebug.Log($"Increase save data slot from {oldSaveDataSlot} to {newSaveDataSlot}... OKAY");
    }

    // 저장 슬롯 감소 (불러오기 실패 후 항상 1씩 감소되어야 함)
    public static void DecreaseSaveDataSlotAndWrite()
    {
        var oldSaveDataSlot = GetSaveSlot();
        var newSaveDataSlot = PositiveMod(oldSaveDataSlot - 1, maxSaveDataSlot);
        ConDebug.Log($"Decrease save data slot from {oldSaveDataSlot} to {newSaveDataSlot}...");
        PlayerPrefs.SetInt(saveDataSlotKey, newSaveDataSlot);
        PlayerPrefs.Save();
        ConDebug.Log($"Decrease save data slot from {oldSaveDataSlot} to {newSaveDataSlot}... OKAY");
    }

    static void ResetSaveDataSlotAndWrite()
    {
        lastSaveDataArray = null;
        PlayerPrefs.SetInt(saveDataSlotKey, 0);
        PlayerPrefs.Save();
    }

    internal static void DeleteSaveFileAndReloadScene()
    {
        // From MSDN: If the file to be deleted does not exist, no exception is thrown.
        ConDebug.Log("DeleteSaveFileAndReloadScene");
        DeleteAllSaveFiles();
        Splash.LoadSplashScene();
    }

    string IPlatformSaveLoadManager.GetLoadOverwriteConfirmMessage(byte[] bytes)
    {
        return BlackPlatform.GetLoadOverwriteConfirmMessage(BlackPlatform.instance.GetCloudMetadataFromBytes(bytes));
    }

    string IPlatformSaveLoadManager.GetSaveOverwriteConfirmMessage(byte[] bytes)
    {
        return BlackPlatform.GetSaveOverwriteConfirmMessage(BlackPlatform.instance.GetCloudMetadataFromBytes(bytes));
    }

    bool IPlatformSaveLoadManager.IsLoadRollback(byte[] bytes)
    {
        return BlackPlatform.IsLoadRollback(BlackPlatform.instance.GetCloudMetadataFromBytes(bytes));
    }

    bool IPlatformSaveLoadManager.IsSaveRollback(byte[] bytes)
    {
        return BlackPlatform.IsSaveRollback(BlackPlatform.instance.GetCloudMetadataFromBytes(bytes));
    }

    void IPlatformSaveLoadManager.SaveBeforeCloudSave()
    {
        BlackPlatform.instance.SaveBeforeCloudSave();
    }

    public static void DeleteAllSaveFiles()
    {
        for (int i = 0; i < maxSaveDataSlot; i++)
        {
            File.Delete(GetSaveLoadFilePathName(i));
        }

        ResetSaveDataSlotAndWrite();
    }

    public bool Save(IBlackContext context, ConfigPopup configPopup, Sound sound, Data data)
    {
        // 에디터에서 간혹 게임 플레이 시작할 때 Load도 호출되기도 전에 Save가 먼저 호출되기도 한다.
        // (OnApplicationPause 통해서)
        // 실제 기기에서도 이럴 수 있나? 이러면 망인데...
        // 그래서 플래그를 하나 추가한다. 이 플래그는 로드가 제대로 한번 됐을 때 true로 변경된다.
        if (context == null || context.LoadedAtLeastOnce == false)
        {
            Debug.LogWarning(
                "****** Save() called before first Load(). There might be an error during Load(). Save() will be skipped to prevent losing your save data.");
            return false;
        }

        var blackSaveData2 = new BlackSaveData2();

        return SaveBlackSaveData2(blackSaveData2);
    }

    static void WriteAllBytesAtomically(string filePath, byte[] bytes)
    {
        var temporaryPath = CreateNewTempPath();
        using (var tempFile = File.Create(temporaryPath, 4 * 1024, FileOptions.WriteThrough))
        {
            tempFile.Write(bytes, 0, bytes.Length);
            tempFile.Close();
        }

        File.Delete(filePath);
        File.Move(temporaryPath, filePath);
    }

    static string CreateNewTempPath()
    {
        return Path.Combine(Application.temporaryCachePath, Guid.NewGuid().ToString());
    }

    public static bool SaveBlackSaveData2(BlackSaveData2 blackSaveData2)
    {
        var binFormatter = new BinaryFormatter();
        var memStream = new MemoryStream();
        //ConDebug.LogFormat("Start Saving JSON Data: {0}", JsonUtility.ToJson(blackSaveData2));
        binFormatter.Serialize(memStream, blackSaveData2);
        var saveDataArray = memStream.ToArray();
        ConDebug.LogFormat("Saving path: {0}", SaveFileName);
        if (lastSaveDataArray != null && lastSaveDataArray.SequenceEqual(saveDataArray))
        {
            ConDebug.LogFormat("Saving skipped since there is no difference made compared to last time saved.");
        }
        else
        {
            try
            {
                // 진짜 쓰자!!
                WriteAllBytesAtomically(SaveFileName, saveDataArray);

                // 마지막 저장 데이터 갱신
                lastSaveDataArray = saveDataArray;
                ConDebug.Log($"{SaveFileName} Saved. (written to disk)");

                // 유저 서비스를 위해 필요할 수도 있으니까 개발 중일 때는 base64 인코딩 버전 세이브 파일도 저장한다.
                // 실서비스 버전에서는 불필요한 기능이다.
                if (Application.isEditor)
                {
                    var base64Path = SaveFileName + ".base64.txt";
                    ConDebug.LogFormat("Saving path (base64): {0}", base64Path);
                    File.WriteAllText(base64Path, Convert.ToBase64String(saveDataArray));
                    ConDebug.Log($"{base64Path} Saved. (written to disk)");
                }

                IncreaseSaveDataSlotAndWrite();
                var lastBlackLevel = blackSaveData2.lastClearedStageId;
                var gem = (blackSaveData2.freeGemScUInt128 + blackSaveData2.paidGemScUInt128).ToUInt128()
                    .ToClampedLong();
                BlackLogManager.Add(BlackLogEntry.Type.GameSaved, lastBlackLevel, gem);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("Writing to disk failed!!!");
                ConfirmPopup.instance.Open("Writing to disk failed!!!");
                BlackLogManager.Add(BlackLogEntry.Type.GameSaveFailure, 0, 0);
                return false;
            }
        }

        return true;
    }

    static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
    {
        if (val.CompareTo(min) < 0) return min;
        else if (val.CompareTo(max) > 0) return max;
        else return val;
    }

    public static void Load(IBlackContext context)
    {
        // 모든 세이브 슬롯에 대해 로드를 성공 할 때까지 시도한다.
        List<Exception> exceptionList = new List<Exception>();

        for (int i = 0; i < maxSaveDataSlot; i++)
        {
            try
            {
                if (LoadInternal(context))
                {
                    // 저장 파일 중 하나는 제대로 읽히긴 했다.
                    if (i != 0)
                    {
                        // 그런데 한번 이상 실패했다면 에러 메시지는 보여준다.
                        Debug.LogError($"Save data rolled back {i} time(s)...!!!");
                    }

                    // 게임은 속행하면 된다. 롤백이 됐건 안됐건 읽긴 읽었다...
                    return;
                }
                else
                {
                    // 뭔가 예외 발생에 의한 실패는 아니지만 실패일 수도 있다.
                    // 어쩄든 실패긴 실패.
                    // 이전 슬롯으로 넘어간다.
                    exceptionList.Add(new Exception("Black Save Data Load Exception"));
                    DecreaseSaveDataSlotAndWrite();
                }
            }
            catch (NotSupportedBlackSaveDataVersionException e)
            {
                // 지원되지 않는 저장 파일 버전
                Debug.LogWarning(e.ToString());
                exceptionList.Add(e);
                DecreaseSaveDataSlotAndWrite();
            }
            catch (SaveFileNotFoundException e)
            {
                // 세이브 파일 자체가 없네?
                Debug.LogWarning(e.ToString());
                exceptionList.Add(e);
                DecreaseSaveDataSlotAndWrite();
            }
            catch (PurchaseCountBanException e)
            {
                // BAN
                Debug.LogWarning(e.ToString());
                exceptionList.Add(e);
                break;
            }
            catch (LocalUserIdBanException e)
            {
                // BAN
                Debug.LogWarning(e.ToString());
                exceptionList.Add(e);
                break;
            }
            catch (Exception e)
            {
                // 세이브 파일 읽는 도중 알 수 없는 예외 발생
                // 무언가 크게 잘못됐어...
                // 큰일이다~~
                // 이전 슬롯으로 넘어간다.
                Debug.LogException(e);
                exceptionList.Add(e);
                DecreaseSaveDataSlotAndWrite();
                BlackLogManager.Add(BlackLogEntry.Type.GameLoadFailure, 0, GetSaveSlot());
            }
        }

        if (exceptionList.All(e => e.GetType() == typeof(SaveFileNotFoundException)))
        {
            // 세이브 파일이 하나도 없다.
            // 신규 유저다~~~ 풍악을 울려라~~~~~
            ProcessNewUser(context, exceptionList[0]);
        }
        else if (exceptionList.Any(e => e.GetType() == typeof(NotSupportedBlackSaveDataVersionException)))
        {
            var exception = (NotSupportedBlackSaveDataVersionException) exceptionList.FirstOrDefault(e =>
                e.GetType() == typeof(NotSupportedBlackSaveDataVersionException));
            if (exception != null)
            {
                // 새 버전으로 업그레이드하면 해결되는 문제다.
                ProcessCriticalLoadErrorPrelude(exceptionList);
                ProcessUpdateNeededError(exception.SaveFileVersion);
            }
            else
            {
                Debug.LogError("...?");
                ProcessWtfError(exceptionList);
            }
        }
        else
        {
            Debug.LogError("All save files cannot be loaded....T.T");
            ProcessWtfError(exceptionList);
        }
    }

    static void ProcessWtfError(List<Exception> exceptionList)
    {
        // W.T.F.
        var st = ProcessCriticalLoadErrorPrelude(exceptionList);
        ProcessCriticalLoadError(exceptionList, st);
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    static bool LoadInternal(IBlackContext context)
    {
        var blackSaveData2 = LoadBlackSaveData2();

        // 세이브 데이터 자체에 오류가 있는 케이스이다.
        if (blackSaveData2.version < 1)
        {
            return false;
        }

        var oldVersion = blackSaveData2.version;
        // 최신 버전 데이터로 마이그레이션
        MigrateBlackSaveData2(blackSaveData2);

        if (blackSaveData2.version == LatestVersion)
        {
            // GOOD!
        }
        else if (blackSaveData2.version > LatestVersion)
        {
            if (Application.isEditor)
            {
                Debug.LogError(
                    "NotSupportedBlackSaveDataVersionException should be thrown at this point in devices. In editor, you can proceed without error...");
            }
            else
            {
                // 저장 파일 버전이 더 높다? 아마도 최신 버전에서 저장한 클라우드 저장 파일을 예전 버전 클라이언트에서 클라우드 불러오기 한 듯
                throw new NotSupportedBlackSaveDataVersionException(blackSaveData2.version);
            }
        }
        else
        {
            throw new Exception(
                $"[CRITICAL ERROR] Latest version {LatestVersion} not match save file latest version field {blackSaveData2.version}!!!");
        }

        // 치트 모드 판별 여부에 따라 아래 코드의 작동이 달라진다.
        // 최대한 먼저 하자.
        // (예를 들어 context.LastDailyRewardRedeemedIndex 대입 시 리더보드 등록을 할 것인지 말 것인지 등)
        context.CheatMode = blackSaveData2.cheatMode;
        context.WaiveBan = blackSaveData2.waiveBan;

        // 부정 이용자 검출되기라도 한다면 이 정보가 먼저 필요하므로 먼저 로드하자.
        if (blackSaveData2.userPseudoId <= 0)
        {
            blackSaveData2.userPseudoId = NewUserPseudoId();
        }

        context.UserPseudoId = blackSaveData2.userPseudoId;
        context.LastConsumedServiceIndex = blackSaveData2.lastConsumedServiceIndex;

        // 부정 이용 사용자 걸러낸다.
        // 다만, 부정 이용 사용자가 아닌데 걸러진 경우 개발팀 문의를 통해 풀 수 있다.
        // 그렇게 풀린 유저는 context.waiveBan이 true를 해 주기로 한다.
        // 그렇다면 이 루틴은 아예 작동하지 않는다.
        if (context.WaiveBan == false)
        {
            if (blackSaveData2.purchasedProductDict != null)
            {
                var totalPurchaseCount = blackSaveData2.purchasedProductDict.Values.Sum(e => e.ToInt());
                // 1.9.22 버전의 저장 데이터 버전인 경우에만 다수의 인앱 결제 횟수 거르기를 한다.
                if (oldVersion <= 30 && totalPurchaseCount > 100)
                {
                    throw new PurchaseCountBanException(totalPurchaseCount);
                }

                // 200회 이상 구매는 버전 상관 없이 무조건 말이 안된다.
                if (totalPurchaseCount > 200)
                {
                    throw new PurchaseCountBanException(totalPurchaseCount);
                }
            }

            var targetIdList = new string[]
            {
            };
            foreach (var targetId in targetIdList)
            {
                if (blackSaveData2.localUserDict != null && blackSaveData2.localUserDict.Keys.Contains(targetId))
                {
                    throw new LocalUserIdBanException(targetId);
                }
            }

            var revokedReceiptList = new string[]
            {
            };
            foreach (var receipt in revokedReceiptList)
            {
                if (blackSaveData2.verifiedProductReceipts != null &&
                    blackSaveData2.verifiedProductReceipts.Contains(receipt))
                {
                    throw new RevokedReceiptException(receipt);
                }
            }
        }

        context.SetRice(blackSaveData2.riceScUInt128);
        context.SetGemZero();
        try
        {
            context.AddFreeGem(blackSaveData2.freeGemScUInt128);
            context.AddPaidGem(blackSaveData2.paidGemScUInt128);
        }
        catch (OverflowException)
        {
            // 1.9.43 버전 이전까지 Paid Gem, Free Gem 값 깎을 때 언더플로 오류가 있었다.
            // 이 상태로 플레이하는덴 문제는 없지만, 이제 오버플로 체크를 하기 때문에
            // 문제가 생겼다.
            // 어디까지 Paid이고 어디까지 Free인지 알 수 없게 되었으므로
            // 이 경우 모든 젬을 Free Gem인 것으로 취급한다.
            var freeGemCopy = blackSaveData2.freeGemScUInt128.ToUInt128();
            var paidGemCopy = blackSaveData2.paidGemScUInt128.ToUInt128();
            UInt128.AddAllowOverflow(out var newGem, ref freeGemCopy, ref paidGemCopy);
            context.SetGemZero();
            context.AddFreeGem(newGem);
            Debug.LogWarning("Gem underflow bug fixed.");
        }

        // 보석 변화 애니메이션 되돌린다.
        var gemBigInt = context.Gem;
        BlackLogManager.Add(BlackLogEntry.Type.GemToLoaded, 0,
            gemBigInt < long.MaxValue ? (long) gemBigInt : long.MaxValue);

        // 슬롯 용량 변화 애니메이션 잠시 끈다.

        context.SetPendingRice(blackSaveData2.pendingRiceScUInt128);
        context.PendingFreeGem = blackSaveData2.pendingFreeGemScUInt128;


        context.StashedRewardJsonList = blackSaveData2.stashedRewardJsonList;

        context.LastDailyRewardRedeemedTicksList =
            blackSaveData2.lastDailyRewardRedeemedTicksList ??
            new List<ScLong> {blackSaveData2.lastDailyRewardRedeemedTicks};
        context.NoAdsCode = blackSaveData2.noAdsCode;

        ConDebug.Log(
            $"Last Daily Reward Redeemed Index {context.LastDailyRewardRedeemedIndex} / DateTime (UTC) {new DateTime(context.LastDailyRewardRedeemedTicks, DateTimeKind.Utc)}");

        context.ApplyPendingRice();
        context.ApplyPendingFreeGem();

        // === Config ===
        Sound.instance.BgmAudioSourceActive = blackSaveData2.muteBgmAudioSource == false;
        Sound.instance.SfxAudioSourceActive = blackSaveData2.muteSfxAudioSource == false;
        Sound.instance.BgmAudioSourceVolume = blackSaveData2.bgmAudioVolume;
        Sound.instance.SfxAudioSourceVolume = blackSaveData2.sfxAudioVolume;

        Sound.instance.BgmAudioVolume = blackSaveData2.bgmAudioVolume;
        Sound.instance.SfxAudioVolume = blackSaveData2.sfxAudioVolume;

        ConfigPopup.instance.IsNotchOn = blackSaveData2.notchSupport;
        ConfigPopup.instance.IsBottomNotchOn = blackSaveData2.bottomNotchSupport;
        ConfigPopup.instance.IsPerformanceModeOn = blackSaveData2.performanceMode;
        ConfigPopup.instance.IsAlwaysOnOn = blackSaveData2.alwaysOn;
        ConfigPopup.instance.IsBigScreenOn = blackSaveData2.bigScreen;

        if (context.CheatMode)
        {
            BlackLogManager.Add(BlackLogEntry.Type.GameCheatEnabled, 0, 0);
        }

        switch (blackSaveData2.languageCode)
        {
            case BlackLanguageCode.Tw:
                ConfigPopup.instance.EnableLanguage(BlackLanguageCode.Tw);
                break;
            case BlackLanguageCode.Ch:
                ConfigPopup.instance.EnableLanguage(BlackLanguageCode.Ch);
                break;
            case BlackLanguageCode.Ja:
                ConfigPopup.instance.EnableLanguage(BlackLanguageCode.Ja);
                break;
            case BlackLanguageCode.En:
                ConfigPopup.instance.EnableLanguage(BlackLanguageCode.En);
                break;
            default:
                ConfigPopup.instance.EnableLanguage(BlackLanguageCode.Ko);
                break;
        }

        context.NoticeData = blackSaveData2.noticeData ?? new NoticeData();
        context.SaveFileLoaded = true;

        if (Application.isEditor)
        {
            Admin.SetNoticeDbPostfixToDev();
        }

        NoticeManager.instance.CheckNoticeSilently();

        // 인앱 상품 구매 내역 디버그 정보
        ConDebug.Log("=== Purchased Begin ===");
        if (blackSaveData2.purchasedProductDict != null)
        {
            foreach (var kv in blackSaveData2.purchasedProductDict)
            {
                ConDebug.Log($"PURCHASED: {kv.Key} = {kv.Value}");
            }
        }

        ConDebug.Log("=== Purchased End ===");

        // 인앱 상품 영수증 디버그 정보
        ConDebug.Log("=== Purchased Receipt ID Begin ===");
        if (blackSaveData2.purchasedProductReceipts != null)
        {
            foreach (var kv in blackSaveData2.purchasedProductReceipts)
            {
                foreach (var kvv in kv.Value)
                {
                    ConDebug.Log($"PURCHASED RECEIPT ID: {kv.Key} = {kvv}");
                }
            }
        }

        ConDebug.Log("=== Purchased Receipt ID End ===");

        // 인앱 상품 영수증 (검증 완료) 디버그 정보
        ConDebug.Log("=== VERIFIED Receipt ID Begin ===");
        if (blackSaveData2.verifiedProductReceipts != null)
        {
            foreach (var v in blackSaveData2.verifiedProductReceipts)
            {
                ConDebug.Log($"\"VERIFIED\" RECEIPT ID (THANK YOU!!!): {v}");
            }
        }

        ConDebug.Log("=== VERIFIED Receipt ID End ===");

        context.LocalUserDict = blackSaveData2.localUserDict;
        if (context.LocalUserDict != null)
        {
            foreach (var kv in context.LocalUserDict)
            {
                ConDebug.Log(kv.Value);
            }
        }

        context.LoadedAtLeastOnce = true;
        BlackLogManager.Add(BlackLogEntry.Type.GameLoaded, context.MahjongLastClearedStageId,
            context.Gem < long.MaxValue ? (long) context.Gem : long.MaxValue);

        return true;
    }

    static void MigrateBlackSaveData2(BlackSaveData2 blackSaveData2)
    {
    }

    public static BlackSaveData2 LoadBlackSaveData2()
    {
        var saveData = new BlackSaveData2
        {
            version = 1
        };
        return saveData;
    }

    static string ProcessCriticalLoadErrorPrelude(List<Exception> exceptionList)
    {
        Debug.LogErrorFormat("Load: Unknown exception thrown: {0}", exceptionList[0]);
        System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
        Debug.LogErrorFormat(t.ToString());
        // 메인 게임 UI 요소를 모두 숨긴다. (아주 심각한 상황. 이 상태로는 무조건 게임 진행은 불가하다.)
        if (BlackContext.instance.CriticalErrorHiddenCanvasList != null)
        {
            foreach (var canvas in BlackContext.instance.CriticalErrorHiddenCanvasList)
            {
                canvas.enabled = false;
            }
        }

        return t.ToString();
    }

    public static void ProcessCriticalLoadError(List<Exception> exceptionList, string st)
    {
        BlackLogManager.Add(BlackLogEntry.Type.GameCriticalError, 0, 0);
        ChangeLanguageBySystemLanguage();
        ConfirmPopup.instance.OpenTwoButtonPopup(
            "\\컬러뮤지엄 저장 파일을 불러오는 중 오류가 발생했습니다.\\n더 심각한 데이터 유실을 막기 위해 게임 플레이는 중단됐습니다.\\n문제 해결을 위해서는 게임 진행 상황이 모두 담긴 파일을 업로드해야 합니다.\\n문제 분석 및 수정이 되면 게임을 재개할 수 있습니다.\\n업로드를 진행하시겠습니까?\\n\\n(인터넷 연결이 필요합니다.)"
                .Localized(), () => UploadSaveFileAsync(exceptionList, st, false),
            () => AskAgainToReportSaveData(exceptionList, st), "\\중대한 오류 발생".Localized(), "\\예".Localized(),
            "\\아니요".Localized());
    }

    static void ProcessUpdateNeededError(int saveFileVersion)
    {
        ChangeLanguageBySystemLanguage();
        ConfirmPopup.instance.Open(
            "\\컬러뮤지엄 업데이트가 필요합니다.\\n\\n확인을 눌러 업데이트하세요!\\n\\n저장 파일 버전: {0}\\n클라이언트 지원 버전: {1}".Localized(saveFileVersion,
                LatestVersion), () =>
            {
                // 컬러뮤지엄 앱 상세 페이지로 보낸다.
                Platform.instance.RequestUserReview();
            });
    }

    static void ChangeLanguageBySystemLanguage()
    {
        switch (Application.systemLanguage)
        {
            case SystemLanguage.Korean:
                ConfigPopup.instance.EnableLanguage(BlackLanguageCode.Ko);
                break;
            case SystemLanguage.Chinese:
            case SystemLanguage.ChineseSimplified:
                ConfigPopup.instance.EnableLanguage(BlackLanguageCode.Ch);
                break;
            case SystemLanguage.ChineseTraditional:
                ConfigPopup.instance.EnableLanguage(BlackLanguageCode.Tw);
                break;
            case SystemLanguage.Japanese:
                ConfigPopup.instance.EnableLanguage(BlackLanguageCode.Ja);
                break;
            case SystemLanguage.English:
                ConfigPopup.instance.EnableLanguage(BlackLanguageCode.En);
                break;
            default:
                ConDebug.Log($"Not supported system language {Application.systemLanguage}. Fallback to English...");
                ConfigPopup.instance.EnableLanguage(BlackLanguageCode.En);
                break;
        }
    }

    public static void EnterRecoveryCode(List<Exception> exceptionList, string st, bool notCriticalError)
    {
        ConfirmPopup.instance.OpenInputFieldPopup("\\안내 받은 복구 코드를 입력해 주십시오.".Localized(), () =>
        {
            ConfirmPopup.instance.Close();
            ErrorReporter.instance.ProcessRecoveryCode(exceptionList, st, ConfirmPopup.instance.InputFieldText);
        }, () =>
        {
            if (notCriticalError == false)
            {
                ProcessCriticalLoadError(exceptionList, st);
            }
            else
            {
                ConfirmPopup.instance.Close();
            }
        }, "\\복구 코드".Localized(), Header.Normal, "", "");
    }

    static void AskAgainToReportSaveData(List<Exception> exceptionList, string st)
    {
        ConfirmPopup.instance.OpenTwoButtonPopup(
            "\\진행 상황을 업로드하지 않으면 더이상 게임을 진행할 수 없습니다.\\n\\n업로드가 불가한 증상의 경우 네이버 공식 카페로 이동하여 해결 방법에 대해 문의해 주시기 바랍니다."
                .Localized(), () =>
            {
                ConfigPopup.instance.OpenCommunity();
                ProcessCriticalLoadError(exceptionList, st);
            }, () => EnterRecoveryCode(exceptionList, st, false), "\\중대한 오류 발생".Localized(), "\\공식 카페 이동".Localized(),
            "\\복구 코드 입력".Localized());
    }

    static async void UploadSaveFileAsync(List<Exception> exceptionList, string st, bool notCriticalError)
    {
        ConfirmPopup.instance.Close();
        await ErrorReporter.instance.UploadSaveFileIncidentAsync(exceptionList, st, notCriticalError);
    }

    static void ProcessNewUser(IBlackContext context, Exception e)
    {
        ConDebug.LogFormat("Load: Save file not found: {0}", e.ToString());
        ResetData(context);
        ConDebug.Log("Your OS language is " + Application.systemLanguage);
        ChangeLanguageBySystemLanguage();
        ShowFirstInstallWelcomePopup();
        ConDebug.Log("loadedAtLeastOnce set to true");
        context.LoadedAtLeastOnce = true;
    }

    static void CloseConfirmPopupAndCheckNoticeSilently()
    {
        ConfirmPopup.instance.Close();
        NoticeManager.instance.CheckNoticeSilently();
    }

    static void ShowFirstInstallWelcomePopup()
    {
    }

    static int NewUserPseudoId()
    {
        return BlackRandom.Range(100000000, 1000000000);
    }

    static void ResetData(IBlackContext context)
    {
        context.SetRice(0);
        context.SetGemZero();
        BlackLogManager.Add(BlackLogEntry.Type.GemToZero, 0, 0);
        context.AchievementGathered = new AchievementRecord5(true);
        context.AchievementRedeemed = new AchievementRecord5(false);
        context.UserPseudoId = NewUserPseudoId();
        context.NoticeData = new NoticeData();
        context.LastDailyRewardRedeemedIndex = 0;
        //context.LastDailyRewardRedeemedTicks = 0;
        context.LastConsumedServiceIndex = 0;
        context.SaveFileLoaded = true;

        context.MahjongLastClearedStageId = 0;
        context.MahjongClearTimeList = new List<ScFloat>();
        context.MahjongNextStagePurchased = false;
        context.MahjongCoinAmount = 0;
        context.LastFreeMahjongCoinRefilledTicks = 0;
        context.MahjongSlowMode = false;
        context.MahjongCoinUseCount = 0;
        context.MahjongLastStageFailed = false;

        context.StashedRewardJsonList = new List<ScString>();
        context.LastDailyRewardRedeemedTicksList = new List<ScLong> {0};
        context.NoAdsCode = 0;

        if (SystemInfo.deviceModel.IndexOf("iPhone", StringComparison.Ordinal) >= 0)
        {
            var screenRatio = (1.0 * Screen.height) / (1.0 * Screen.width);
            if (2.1 < screenRatio && screenRatio < 2.2)
            {
                ConfigPopup.instance.IsNotchOn = true;
            }
            else
            {
                ConfigPopup.instance.IsNotchOn = false;
            }
        }
        else
        {
            ConfigPopup.instance.IsNotchOn = false;
        }

        // 아마 상단 노치가 필요한 모델은 하단도 필요하겠지...?
        ConfigPopup.instance.IsBottomNotchOn = ConfigPopup.instance.IsNotchOn;

        if (Application.isMobilePlatform == false)
        {
            ConfigPopup.instance.IsPerformanceModeOn = true;
        }

        BlackLogManager.Add(BlackLogEntry.Type.GameReset, 0, 0);
    }
}