using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Dict = System.Collections.Generic.Dictionary<string, object>;
using System.Linq;
using ConditionalDebug;

public class BlackLogViewer : MonoBehaviour
{
    [SerializeField]
    Text logText;

    [SerializeField]
    Text pageText;

    [SerializeField]
    int curPage = 0;

    [SerializeField]
    int pages = 0;

    [SerializeField]
    int entryCountPerPage = 20;

    [SerializeField]
    int totalEntryCount = 0;

    [SerializeField]
    GameObject loadRemoteLogButton;

    IBlackLogSource logSource;

    public interface IBlackLogSource
    {
        List<BlackLogEntry> Read(int startOffset, int logEntryStartIndex, int count);
        long Count();
        void Flush();
    }

    void OnEnable()
    {
        logSource = BlackLogManager.instance;
        UpdateToPage(0);
        loadRemoteLogButton.SetActive(BlackSpawner.instance.CheatMode);
    }

    void UpdateToPage(int page)
    {
        if (gameObject.activeSelf == false)
        {
            return;
        }

        FlushAndUpdateTotalPages();
        curPage = pages > 0 ? Mathf.Clamp(page, 0, pages - 1) : 0;
        RefreshCurrentPage();
    }

    void RefreshCurrentPage()
    {
        if (pages > 0)
        {
            StringBuilder sb = new StringBuilder();
            var logEntryList = logSource.Read(LogFileReadOffset, curPage * entryCountPerPage, entryCountPerPage);
            //ConDebug.Log($"log Entry List: {logEntryList.Count}");
            foreach (var logEntry in logEntryList)
            {
                sb.AppendLine(logEntry.ToColoredString());
            }

            logText.text = sb.ToString();
        }
        else
        {
            logText.text = "EMPTY";
        }

        RefreshPageText();
    }

    void FlushAndUpdateTotalPages()
    {
        logSource.Flush();
        totalEntryCount = (int) logSource.Count();
        pages = totalEntryCount / entryCountPerPage + (totalEntryCount % entryCountPerPage == 0 ? 0 : 1);
    }

    public void GoToFirstPage()
    {
        UpdateToPage(0);
    }

    public void GoToPrevPage()
    {
        UpdateToPage(curPage - 1);
    }

    public void GoToNextPage()
    {
        UpdateToPage(curPage + 1);
    }

    public void GoToLastPage()
    {
        UpdateToPage(pages - 1);
    }

    void RefreshPageText()
    {
        if (pages > 0)
        {
            pageText.text = $"{curPage + 1}/{pages}";
        }
        else
        {
            pageText.text = "--/--";
        }
    }

    public void Refresh()
    {
        if (gameObject.activeSelf == false)
        {
            return;
        }

        FlushAndUpdateTotalPages();
        // 맨 뒤 페이지로 간다...? 자동으로?? 뭔가 불편할 듯
        //curPage = pages > 0 ? pages - 1 : 0;
        RefreshCurrentPage();
    }

    class BlackRemoteLogSource : IBlackLogSource
    {
        MemoryStream readLogStream;

        public void Flush()
        {
        }

        public long Count()
        {
            var dummyLogEntryBytes = BlackLogManager.GetLogEntryBytes(BlackLogEntry.Type.GameLoaded, 0, 0);
            return readLogStream.Length / dummyLogEntryBytes.Length;
        }

        public List<BlackLogEntry> Read(int startOffset, int logEntryStartIndex, int count)
        {
            List<BlackLogEntry> logEntryList = new List<BlackLogEntry>();
            var dummyLogEntryBytes = BlackLogManager.GetLogEntryBytes(BlackLogEntry.Type.GameLoaded, 0, 0);
            readLogStream.Seek(startOffset + logEntryStartIndex * dummyLogEntryBytes.Length, SeekOrigin.Begin);
            var bytes = new byte[count * dummyLogEntryBytes.Length];
            var readByteCount = readLogStream.Read(bytes, 0, bytes.Length);
            var offset = 0;
            for (int i = 0; i < readByteCount / dummyLogEntryBytes.Length; i++)
            {
                logEntryList.Add(new BlackLogEntry
                {
                    ticks = BitConverter.ToInt64(bytes, offset + 0),
                    type = BitConverter.ToInt32(bytes, offset + 0 + 8),
                    arg1 = BitConverter.ToInt32(bytes, offset + 0 + 8 + 4),
                    arg2 = BitConverter.ToInt64(bytes, offset + 0 + 8 + 4 + 4),
                });
                offset += dummyLogEntryBytes.Length;
            }

            return logEntryList;
        }

        public async Task LoadPlayLogAsync(string playLogCode)
        {
            var saveDbUrl = ConfigPopup.BaseUrl + "/playLog";
            ProgressMessage.instance.Open("Loading play log");
            playLogCode = playLogCode.Trim();
            var url = string.Format("{0}/{1}", saveDbUrl, playLogCode);
            ConDebug.LogFormat("URL: {0}", url);

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var getTask = await httpClient.GetAsync(url);
                    if (getTask.IsSuccessStatusCode)
                    {
                        var text = await getTask.Content.ReadAsStringAsync();
                        var userPlayLogDataRoot = MiniJSON.Json.Deserialize(text) as Dict;
                        var userPlayLogDataFields = userPlayLogDataRoot["fields"] as Dict;
                        var userPlayLogDataFieldsSaveData = userPlayLogDataFields["playLogData"] as Dict;
                        var userPlayLogDataFieldsSaveDataStringValue =
                            userPlayLogDataFieldsSaveData["bytesValue"] as string;
                        var userPlayLogUncompressedSizeData =
                            userPlayLogDataFields["playLogUncompressedSizeData"] as Dict;
                        var userPlayLogUncompressedSizeDataIntegerValue =
                            int.Parse(userPlayLogUncompressedSizeData["integerValue"] as string);

                        var playLogDataBase64 = userPlayLogDataFieldsSaveDataStringValue;
                        var playLogData = Convert.FromBase64String(playLogDataBase64);
                        var playLogUncompressedData = new byte[userPlayLogUncompressedSizeDataIntegerValue];
                        MessagePack.LZ4.LZ4Codec.Decode(playLogData, 0, playLogData.Length, playLogUncompressedData, 0,
                            playLogUncompressedData.Length);

                        readLogStream = new MemoryStream(playLogUncompressedData);

                        ConDebug.LogFormat("Play Log Data Base64 ({0} bytes): {1}",
                            playLogDataBase64 != null ? playLogDataBase64.Length : 0, playLogDataBase64);
                        ConDebug.LogFormat("Play Log Data ({0} bytes - compresseed)", playLogData.Length);
                    }
                    else
                    {
                        Debug.LogError($"Loading play log failed - status code {getTask.StatusCode}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Play log upload exception: {e.ToString()}");
            }
            finally
            {
                // 어떤 경우가 됐든지 마지막으로는 진행 상황 창을 닫아야 한다.
                ProgressMessage.instance.Close();
            }
        }
    }

    // 원격 로그 데이터 읽을 때 보면 원격 로그가 밀려서 들어오는 경우가 있다.
    // 주로 16바이트... 여튼, 문제를 제대로 해결하기 전까지는 이 값을 바꿔서 임시 대응한다.
    const int LogFileReadOffset = 0;

    public void LoadRemoteLog()
    {
        ConfirmPopup.instance.OpenInputFieldPopup("Play Log Error Device ID", async () =>
        {
            ConfirmPopup.instance.Close();
            var remoteLogSource = new BlackRemoteLogSource();
            await remoteLogSource.LoadPlayLogAsync(ConfirmPopup.instance.InputFieldText);
            logSource = remoteLogSource;

            var allEntries = logSource.Read(LogFileReadOffset, 0, (int) logSource.Count());
            var span = (new DateTime(allEntries.Last().ticks) - new DateTime(allEntries[0].ticks));
            ConDebug.Log($"LoadRemoteLog: Time Span = {span}");
            UpdateToPage(0);

            StringBuilder sb = new StringBuilder();
            foreach (var logEntry in allEntries)
            {
                sb.AppendLine(logEntry.ToTabbedString());
            }

            File.WriteAllText("remotelog.txt", sb.ToString());
        }, () => { ConfirmPopup.instance.Close(); }, "Remote Log", Header.Normal, "", "");
    }
}