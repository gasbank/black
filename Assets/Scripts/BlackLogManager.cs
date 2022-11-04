using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ConditionalDebug;
using MessagePack.LZ4;
using UnityEngine;

public class BlackLogManager : MonoBehaviour, BlackLogViewer.IBlackLogSource, IPlatformLogManager
{
    public static BlackLogManager Instance;

    [SerializeField]
    BlackLogViewer logViewer;

    FileStream writeLogStream;

    string LogFilePath => Path.Combine(Application.persistentDataPath, "black.log");

    public void Flush()
    {
        if (writeLogStream != null) writeLogStream.Flush(true);
    }

    public long Count()
    {
        var dummyLogEntryBytes = GetLogEntryBytes(BlackLogEntry.Type.GameLoaded, 0, 0);
        using (var readLogStream = OpenReadLogStream())
        {
            return readLogStream.Length / dummyLogEntryBytes.Length;
        }
    }

    public List<BlackLogEntry> Read(int startOffset, int logEntryStartIndex, int count)
    {
        var logEntryList = new List<BlackLogEntry>();
        using (var readLogStream = OpenReadLogStream())
        {
            var dummyLogEntryBytes = GetLogEntryBytes(BlackLogEntry.Type.GameLoaded, 0, 0);
            readLogStream.Seek(startOffset + logEntryStartIndex * dummyLogEntryBytes.Length, SeekOrigin.Begin);
            var bytes = new byte[count * dummyLogEntryBytes.Length];
            var readByteCount = readLogStream.Read(bytes, 0, bytes.Length);
            var offset = 0;
            for (var i = 0; i < readByteCount / dummyLogEntryBytes.Length; i++)
            {
                logEntryList.Add(new BlackLogEntry
                {
                    ticks = BitConverter.ToInt64(bytes, offset + 0),
                    type = BitConverter.ToInt32(bytes, offset + 0 + 8),
                    arg1 = BitConverter.ToInt32(bytes, offset + 0 + 8 + 4),
                    arg2 = BitConverter.ToInt64(bytes, offset + 0 + 8 + 4 + 4)
                });
                offset += dummyLogEntryBytes.Length;
            }
        }

        return logEntryList;
    }

    public void Add(int logType, int arg0, int arg1)
    {
        Add((BlackLogEntry.Type) logType, arg0, arg1);
    }

    void Awake()
    {
        Instance = this;
        writeLogStream = File.Open(LogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
    }

    void OnApplicationPause(bool pause)
    {
        writeLogStream.Flush(true);
    }

    void OnDestroy()
    {
        FinalizeLogStream();
    }

    void OnApplicationQuit()
    {
        FinalizeLogStream();
    }

    void FinalizeLogStream()
    {
        if (writeLogStream != null)
        {
            var deleteAfterClosed = false;
            try
            {
                Flush();
            }
            catch (IOException e) /* when ((e.HResult & 0xFFFF) == 0x27 || (e.HResult & 0xFFFF) == 0x70) */
            {
                Debug.LogError($"Log flush failed with exception {e.Message}. Log file will be deleted.");
                deleteAfterClosed = true;
            }
            finally
            {
                writeLogStream.Close();
                writeLogStream.Dispose();
                writeLogStream = null;
                if (deleteAfterClosed)
                {
                    File.Delete(LogFilePath);
                    Debug.LogError($"Log file ${LogFilePath} deleted.");
                }
            }
        }
    }

    public static void Add(BlackLogEntry.Type type, int arg1, long arg2)
    {
        try
        {
            if (Instance != null && Instance.writeLogStream != null)
            {
                var logBytes = GetLogEntryBytes(type, arg1, arg2);
                Instance.writeLogStream.Write(logBytes, 0, logBytes.Length);
                // 로그 항목이 생길 때마다 UI를 업데이트하는 것은 일반적인 유저에게는 불필요한 일이다.
                if (Application.isEditor && Instance.logViewer != null) Instance.logViewer.Refresh();
            }
        }
        catch
        {
        }
    }

    public static byte[] GetLogEntryBytes(BlackLogEntry.Type type, int arg1, long arg2)
    {
        return BitConverter.GetBytes(DateTime.UtcNow.Ticks)
            .Concat(BitConverter.GetBytes((int) type))
            .Concat(BitConverter.GetBytes(arg1))
            .Concat(BitConverter.GetBytes(arg2)).ToArray();
    }

    FileStream OpenReadLogStream()
    {
        return File.Open(Instance.LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    }

    public async Task<string> UploadPlayLogAsync(byte[] playLogBytes, int uncompressedBytesLength,
        string progressMessage, string receiptDomain, string receiptId, string receipt)
    {
        var hasReceipt = string.IsNullOrEmpty(receiptDomain) == false && string.IsNullOrEmpty(receiptId) == false &&
                         string.IsNullOrEmpty(receipt) == false;
        var uploadPlayLogFileOrReceiptDbUrl = ConfigPopup.BaseUrl + "/" + (hasReceipt ? receiptDomain : "playLog");
        var ownProgressMessage = string.IsNullOrEmpty(progressMessage) == false;
        if (ownProgressMessage) ProgressMessage.Instance.Open(progressMessage);

        var errorDeviceId = ErrorReporter.Instance.GetOrCreateErrorDeviceId();
        var url = string.Format("{0}/{1}", uploadPlayLogFileOrReceiptDbUrl, hasReceipt ? receiptId : errorDeviceId);
        var saveFile = new PlayLogFile();
        var uploadDate = DateTime.UtcNow;
        try
        {
            saveFile.fields.uploadDate.timestampValue = uploadDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
        catch
        {
            saveFile.fields.uploadDate.timestampValue = "ERROR";
        }

        try
        {
            saveFile.fields.appMetaInfo.stringValue = ConfigPopup.GetAppMetaInfo();
        }
        catch
        {
            saveFile.fields.appMetaInfo.stringValue = "ERROR";
        }

        try
        {
            saveFile.fields.userId.stringValue = ConfigPopup.GetUserId();
        }
        catch
        {
            saveFile.fields.userId.stringValue = "ERROR";
        }

        try
        {
            saveFile.fields.saveData.bytesValue =
                Convert.ToBase64String(File.ReadAllBytes(SaveLoadManager.LoadFileName));
        }
        catch
        {
            saveFile.fields.saveData.bytesValue = "ERROR";
        }

        try
        {
            saveFile.fields.playLogData.bytesValue = Convert.ToBase64String(playLogBytes);
        }
        catch
        {
            saveFile.fields.playLogData.bytesValue = "ERROR";
        }

        try
        {
            saveFile.fields.playLogUncompressedSizeData.integerValue = uncompressedBytesLength;
        }
        catch
        {
            saveFile.fields.playLogUncompressedSizeData.integerValue = 0;
        }

        try
        {
            saveFile.fields.gemStr.stringValue = BlackContext.Instance.Gem.ToString("n0");
        }
        catch
        {
            saveFile.fields.gemStr.stringValue = "ERROR";
        }

        try
        {
            saveFile.fields.socialUserName.stringValue = Social.localUser.userName;
        }
        catch
        {
            saveFile.fields.socialUserName.stringValue = "ERROR";
        }

        try
        {
            saveFile.fields.receiptDomain.stringValue = receiptDomain;
        }
        catch
        {
            saveFile.fields.receiptDomain.stringValue = "ERROR";
        }

        try
        {
            saveFile.fields.receiptId.stringValue = receiptId;
        }
        catch
        {
            saveFile.fields.receiptId.stringValue = "ERROR";
        }

        try
        {
            saveFile.fields.receipt.stringValue = receipt;
        }
        catch
        {
            saveFile.fields.receipt.stringValue = "ERROR";
        }

        var reasonPhrase = "";
        try
        {
            using (var httpClient = new HttpClient())
            {
                var patchData = JsonUtility.ToJson(saveFile);
                using (var patchContent = new StringContent(patchData))
                {
                    ConDebug.Log($"HttpClient PATCH TO {url}...");

                    // PATCH 시작하고 기다린다.
                    var patchTask = await httpClient.PatchAsync(new Uri(url), patchContent);

                    ConDebug.Log($"HttpClient Result: {patchTask.ReasonPhrase}");

                    if (patchTask.IsSuccessStatusCode)
                    {
                        ConDebug.Log("Play log uploaded successfully.");
                        //var msg = string.Format("\\업로드가 성공적으로 완료됐습니다.\\n\\n업로드 코드: {0}\\n용량: {1:n0}바이트\\nTS: {2}\\n\\n<color=brown>본 화면의 스크린샷을 찍어 공식 카페에 버그 신고를 부탁 드립니다.</color>\\n\\n업로드된 데이터를 분석 후, 카페를 통해 이후 진행을 안내드리겠습니다.\\n\\n공식 카페로 이동하거나, 안내 받은 복구 코드를 입력하세요.".Localized(), errorDeviceId, patchData.Length, saveFile.fields.uploadDate.timestampValue);
                        //ConfirmPopup.Instance.OpenTwoButtonPopup(msg, () => ConfigPopup.Instance.OpenCommunity(), () => SaveLoadManager.EnterRecoveryCode(exceptionList, st), "\\업로드 완료".Localized(), "\\공식 카페 이동".Localized(), "\\복구 코드 입력".Localized());
                    }
                    else
                    {
                        Debug.LogError($"Play log upload failed: status code={patchTask.StatusCode}");
                        reasonPhrase = patchTask.ReasonPhrase;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Play log upload exception: {e}");
        }
        finally
        {
            if (ownProgressMessage)
                // 어떤 경우가 됐든지 마지막으로는 진행 상황 창을 닫아야 한다.
                ProgressMessage.Instance.Close();
        }

        return reasonPhrase;
    }

    public static async Task<string> DumpAndUploadPlayLog(string progressMessage, string receiptDomain,
        string receiptId, string receipt)
    {
        // 개발용 버전에서는 좀 더 많이 가져오자.
        // 근데 아주아주 많이 가져올 수는 없다. Firebase 데이터베이스 노드별 용량 한계가 있다.
#if DEV_BUILD
        const int count = 50000;
#else
        const int count = 10000;
#endif
        ConDebug.Log($"Dumping log for last {count:n0} log entries...");
        var dummyLogEntryBytes = GetLogEntryBytes(BlackLogEntry.Type.GameLoaded, 0, 0);
        using (var readLogStream = Instance.OpenReadLogStream())
        {
            readLogStream.Seek(Math.Max(0, readLogStream.Length - count * dummyLogEntryBytes.Length), SeekOrigin.Begin);
            var bytes = new byte[count * dummyLogEntryBytes.Length];
            var readByteCount = readLogStream.Read(bytes, 0, bytes.Length);
            ConDebug.Log(
                $"{readByteCount:n0} bytes ({readByteCount / dummyLogEntryBytes.Length:n0} log entries) read.");
            if (readByteCount % dummyLogEntryBytes.Length != 0)
            {
                var reasonPhrase =
                    $"Log dump failed! readByteCount={readByteCount:n0}, logEntrySize={dummyLogEntryBytes.Length:n0}. Abort!";
                Debug.LogError(reasonPhrase);
                return reasonPhrase;
            }

            var maxOutBytesLength = LZ4Codec.MaximumOutputLength(readByteCount);
            ConDebug.Log($"Maximum compressed log: {maxOutBytesLength:n0} bytes");
            var outBytes = new byte[maxOutBytesLength];
            var outBytesLength = LZ4Codec.Encode(bytes, 0, readByteCount, outBytes, 0, outBytes.Length);
            ConDebug.Log($"Compressed log size: {outBytesLength:n0} bytes");
            outBytes = outBytes.Take(outBytesLength).ToArray();
            return await Instance.UploadPlayLogAsync(outBytes, readByteCount, progressMessage, receiptDomain, receiptId,
                receipt);
        }
    }

    [Serializable]
    class PlayLogFile
    {
        public Fields fields = new Fields();

        [Serializable]
        public class Fields
        {
            public StringValueData appMetaInfo = new StringValueData();
            public StringValueData gemStr = new StringValueData();
            public BytesValueData playLogData = new BytesValueData();
            public IntegerValueData playLogUncompressedSizeData = new IntegerValueData();
            public StringValueData receipt = new StringValueData();
            public StringValueData receiptDomain = new StringValueData();
            public StringValueData receiptId = new StringValueData();

            public BytesValueData saveData = new BytesValueData();
            public StringValueData socialUserName = new StringValueData();
            public TimestampValueData uploadDate = new TimestampValueData();
            public StringValueData userId = new StringValueData();

            [Serializable]
            public class BytesValueData
            {
                public string bytesValue;
            }

            [Serializable]
            public class TimestampValueData
            {
                public string timestampValue;
            }

            [Serializable]
            public class StringValueData
            {
                public string stringValue;
            }

            [Serializable]
            public class IntegerValueData
            {
                public long integerValue;
            }
        }
    }
}