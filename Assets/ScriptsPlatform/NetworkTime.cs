using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

[DisallowMultipleComponent]
public class NetworkTime : MonoBehaviour
{
    public enum QueryState
    {
        // 처음 앱이 실행됐을 때 상태
        InitialState,

        // NTP 조회 중
        Querying,

        // NTP 조회 최종 실패
        Error,

        // NTP 조회 실패했지만 즉시 재시도할 것
        ErrorButWillRetry,

        // NTP 조회 성공
        NoError
    }

    public const string TimeServerListQueryStartIndexKey = "timeServerListQueryStartIndex";
    static int timeServerListQueryStartIndex = 0;

    // https로 시작하는 것은 HttpClient로 헤더의 Date를 시각으로 쓰고,
    // 그렇지 않은 것은 NTP 이용해서 시각을 가져온다.
    // 우선순위는 조회가 빠른 NTP가 높다.
    static readonly string[] timeServerList =
    {
        "time.windows.com", "time.nist.gov", "time.bora.net", "time.google.com",
        "https://baidu.com", "https://naver.com", "https://google.com"
    };

    static readonly DateTime BaseDateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    static DateTime lastNetworkTime = DateTime.MinValue;

    static double offsetSec = 0;
    static QueryState state = QueryState.InitialState;

    static readonly string STANDARD_NETWORK_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";

    [SerializeField]
    PlatformInterface platformInterface;

    DateTime LastNetworkTime
    {
        get => lastNetworkTime;
        set
        {
            lastNetworkTime = value;
            offsetSec = 0;
        }
    }

    static float LastTimeDotTime { get; set; } = 0;
    public static TimeSpan NetworkTimeDiff { get; private set; }
    public static float TimeDotTimeDiff { get; private set; }
    public static bool TimeDiscrepancyDetected { get; private set; } = false;

    public DateTime EstimatedNetworkTime =>
        State == QueryState.NoError ? LastNetworkTime.AddSeconds(offsetSec) : DateTime.MinValue;

    public int RequestSequence { get; private set; } = 0;

    public QueryState State
    {
        get => state;
        private set
        {
            state = value;
            Publish();
        }
    }

    static int QueryingServerIndex { get; set; } = 0;

    static List<INetworkTimeSubscriber> SubscriberList { get; set; } =
        new List<INetworkTimeSubscriber>();

    public string QueryStateMessageLocalized =>
        PlatformInterface.Instance.text.GetNetworkTimeQueryProgressText(timeServerList.Length,
            QueryingServerIndex + 1);

    async void Awake()
    {
        await Query(false);
    }

    void Update()
    {
        offsetSec += Time.unscaledDeltaTime;
    }

    async void OnApplicationPause(bool pause)
    {
        // 앱에 들어올 때 새로운 쿼리 시작하도록, 이전 것이 안끝났으면 아마 이어서 되겠지?
        if (pause) return;

        if (State == QueryState.ErrorButWillRetry) return;

        // Query 시도는 총 5회, 1초 간격으로
        const int retryCount = 10;
        const int retryIntervalMs = 2000;
        for (var i = 0; i < retryCount; i++)
        {
            await Query(i != retryCount - 1);

            if (State == QueryState.NoError) break;

            if (State == QueryState.Querying) break;

            await Task.Delay(retryIntervalMs);
        }
    }

    void Publish()
    {
        SubscriberList = SubscriberList.Where(e => e.gameObject != null).ToList();
        foreach (var sub in SubscriberList) sub.OnNetworkTimeStateChange(State);
    }

    public void Register(INetworkTimeSubscriber sub)
    {
        PlatformInterface.Instance.logger.Log(
            $"Registering {sub.gameObject.name} to NetworkTime event subscription...");
        SubscriberList.Add(sub);
        PlatformInterface.Instance.logger.Log($"{SubscriberList.Count} subscriber(s).");
    }

    public void Unregister(INetworkTimeSubscriber sub)
    {
        PlatformInterface.Instance.logger.Log(
            $"Unregistering {sub.gameObject.name} to NetworkTime event subscription...");
        SubscriberList.Remove(sub);
        PlatformInterface.Instance.logger.Log($"{SubscriberList.Count} subscriber(s).");
    }

    public async Task Query(bool willRetry)
    {
        // 시간 조회 결론이 나기 전까진 중복 요청할 수 없다.
        if (State == QueryState.Querying)
        {
            PlatformInterface.Instance.logger.Log("Another query is in progress. This Query() call will be aborted.");
            return;
        }

        // 새로운 시간 조회 싸이클을 시작한다.
        timeServerListQueryStartIndex = PlayerPrefs.GetInt(TimeServerListQueryStartIndexKey, 0);
        PlatformInterface.Instance.logger.Log(
            $"ntpServerListQueryStartIndex = {timeServerListQueryStartIndex} [{timeServerList[timeServerListQueryStartIndex % timeServerList.Length]}]");

        for (QueryingServerIndex = 0; QueryingServerIndex < timeServerList.Length; QueryingServerIndex++)
        {
            // 다른 서버 접속시마다 네트워크 시간 구독자에게 알려준다. 조회 중이라고...
            State = QueryState.Querying;
            try
            {
                var newNetworkTime = await GetNetworkTimeAsync();
                var newTimeDotTime = Time.realtimeSinceStartup;

                // 두 번째 이후 조회라면 Time.time 흐른 값과 네트워크 시간 흐른 값을
                // 비교해서 스피드핵 여부를 판별한다.
                if (LastTimeDotTime != 0 && LastNetworkTime != DateTime.MinValue)
                {
                    NetworkTimeDiff = newNetworkTime - LastNetworkTime;
                    TimeDotTimeDiff = newTimeDotTime - LastTimeDotTime;
                    if (NetworkTimeDiff.TotalSeconds != 0)
                    {
                        var discrepancyRatio = TimeDotTimeDiff / NetworkTimeDiff.TotalSeconds;
                        var discrepancy = TimeDotTimeDiff - NetworkTimeDiff.TotalSeconds;
                        PlatformInterface.Instance.logger.LogFormat(
                            "Time discrepancy between Time.time and network time: {0:F3} seconds (ratio={1:F1}%)",
                            discrepancy, discrepancyRatio * 100);
                        if (Mathf.Abs((float) discrepancyRatio) > 2.0f) TimeDiscrepancyDetected = true;
                    }
                    else
                    {
                        Debug.LogError("Network time corrupted.");
                    }
                }

                LastNetworkTime = newNetworkTime;
                LastTimeDotTime = newTimeDotTime;
                // 정상적으로 LastDateTime이 설정됐다면 여기서 끝이다.
                State = QueryState.NoError;
                // 반드시 리턴해야 한다.
                return;
            }
            catch (Exception e)
            {
                LastNetworkTime = DateTime.MinValue;
                Debug.LogWarning(e);
            }
        }

        // 모든 서버 실패
        State = willRetry ? QueryState.ErrorButWillRetry : QueryState.Error;
        Debug.LogWarning("All time servers could not provide network time!");
    }

    public static void ResetTimeServerListQueryStartIndex()
    {
        timeServerListQueryStartIndex = 0;
        PlayerPrefs.SetInt(TimeServerListQueryStartIndexKey, 0);
        PlayerPrefs.Save();
    }

    async Task<DateTime> GetNetworkTimeAsync()
    {
        RequestSequence++;
        var index = timeServerListQueryStartIndex % timeServerList.Length;
        var timeServerAddress = timeServerList[index];
        PlatformInterface.Instance.logger.Log(
            $"GetNetworkTimeAsync() called. Querying from '{timeServerAddress}'... (Req seq = {RequestSequence})");
        try
        {
            DateTime dt;
            if (timeServerAddress.StartsWith("https://"))
                dt = await GetNetworkTimeHttpsServerAsync(timeServerAddress);
            else
                dt = await GetNetworkTimeNtpServerAsync(timeServerAddress);

            // 테스트를 위해 시간 조회가 아주 오래 걸리도록 하고 싶으면
            // 아래 주석을 풀면 된다.
            // await Task.Delay(TimeSpan.FromSeconds(20));

            // 여기까지 왔으면 조회 안전하게 성공했다는 뜻
            PlatformInterface.Instance.logger.Log(
                $"Network time '{dt}' queried from '{timeServerAddress}' successfully. (Req seq = {RequestSequence})");
            return dt;
        }
        catch (Exception e)
        {
            // timeServerListQueryStartIndex로 실패했으니까 다음에 시도할 떄는
            // 그 다음칸으로 해 본다.
            timeServerListQueryStartIndex = (timeServerListQueryStartIndex + 1) % timeServerList.Length;
            Debug.LogWarning(e.ToString());
        }
        finally
        {
            PlayerPrefs.SetInt(TimeServerListQueryStartIndexKey, timeServerListQueryStartIndex);
            PlayerPrefs.Save();
        }

        throw new Exception(
            $"Time server '{timeServerAddress}' unavailable or time query failed. (Req seq = {RequestSequence})");
    }

    async Task<DateTime> GetNetworkTimeHttpsServerAsync(string httpsServer)
    {
        PlatformInterface.Instance.logger.LogFormat("[HTTPS] Querying network time from {0}...", httpsServer);
        using var httpClient = new HttpClient {Timeout = TimeSpan.FromSeconds(2)};
        PlatformInterface.Instance.logger.Log("[HTTPS] Before GetAsync...");
        var getTask = await httpClient.GetAsync(httpsServer, HttpCompletionOption.ResponseHeadersRead);
        PlatformInterface.Instance.logger.Log("[HTTPS] After GetAsync...");
        if (getTask.IsSuccessStatusCode)
        {
            if (getTask.Headers.Date.HasValue)
                return getTask.Headers.Date.Value.UtcDateTime;
            throw new Exception($"No Date available on header from {httpsServer}...");
        }

        throw new Exception($"Invalid response from {httpsServer} ... Result: {getTask.ReasonPhrase}");

        //throw new Exception("Should not reach here");
    }

    // stackoverflow.com/a/3294698/162671
    static uint SwapEndianness(ulong x)
    {
        return (uint) (((x & 0x000000ff) << 24) +
                       ((x & 0x0000ff00) << 8) +
                       ((x & 0x00ff0000) >> 8) +
                       ((x & 0xff000000) >> 24));
    }

    async Task<DateTime> GetNetworkTimeNtpServerAsync(string ntpServer)
    {
        PlatformInterface.Instance.logger.LogFormat("[NTP] Querying network time from {0}...", ntpServer);

        // NTP message size - 16 bytes of the digest (RFC 2030)
        var ntpData = new byte[48];

        //Setting the Leap Indicator, Version Number and Mode values
        ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

        var addresses = (await Dns.GetHostEntryAsync(ntpServer)).AddressList;

        //The UDP port number assigned to NTP is 123
        var ipEndPoint = new IPEndPoint(addresses[0], 123);
        //NTP uses UDP

        using (var udpClient = new UdpClient(123))
        {
            // Async 버전 호출에서는 Timeout 값은 무관하다.
            //udpClient.Client.SendTimeout = 2000;
            //udpClient.Client.ReceiveTimeout = 2000;

            // 이후 있을 Send, Receive 함수에서 매번 서버를 지정하지 않아도 되는 수고를 덜어준다.
            // (접속이란 개념이 원래 UDP에선 없다)
            udpClient.Connect(ipEndPoint);

            await udpClient.SendAsync(ntpData, ntpData.Length);

            var received = await Task.Run(() =>
            {
                var receiveTask = udpClient.ReceiveAsync();
                receiveTask.Wait(2000);
                if (receiveTask.IsCompleted)
                {
                    if (receiveTask.Result.Buffer.Length == ntpData.Length)
                        return receiveTask.Result;
                    throw new Exception(
                        $"Replied NTP packet is {receiveTask.Result.Buffer.Length} bytes! (should be {ntpData.Length})");
                }

                throw new TimeoutException();
            });

            ntpData = received.Buffer;
            udpClient.Close();
        }

        //Offset to get to the "Transmit Timestamp" field (time at which the reply 
        //departed the server for the client, in 64-bit timestamp format."
        const byte serverReplyTime = 40;

        //Get the seconds part
        ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

        //Get the seconds fraction
        ulong fractionPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

        //Convert From big-endian to little-endian
        intPart = SwapEndianness(intPart);
        fractionPart = SwapEndianness(fractionPart);

        var milliseconds = intPart * 1000 + fractionPart * 1000 / 0x100000000L;

        //**UTC** time
        var networkDateTime = BaseDateTime.AddMilliseconds((long) milliseconds);

        return networkDateTime;
    }

    public static DateTime ParseExactUtc(string dateString)
    {
        if (DateTime.TryParseExact(dateString, STANDARD_NETWORK_FORMAT,
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal,
            out var ret))
            return ret.ToUniversalTime();
        return DateTime.MinValue;
    }

    public static string ToString(DateTime dt)
    {
        return dt.ToUniversalTime().ToString(STANDARD_NETWORK_FORMAT);
    }

    public bool EstimatedNetworkTimeInBetween(DateTime begin, DateTime end)
    {
        var estimatedNetworkTime = EstimatedNetworkTime;
        //PlatformInterface.instance.logger.Log($"*** BEGIN: {NetworkTime.ToString(begin)} / CURRENT: {NetworkTime.ToString(estimatedNetworkTime)} / END: {NetworkTime.ToString(end)}");
        return estimatedNetworkTime >= begin && estimatedNetworkTime < end;
    }
}