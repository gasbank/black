using UnityEngine;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif
[DisallowMultipleComponent]
public class PlatformUnityAdsInit : MonoBehaviour
#if UNITY_ADS
    , IUnityAdsListener
#endif
{
    [SerializeField]
    PlatformInterface platformInterface;
    
    public static readonly string PLACEMENT_ID = "rewardedVideo";

    string GAME_ID => PlatformInterface.instance.config.UnityAdsGameId;
    bool USE_TEST_MODE => PlatformInterface.instance.config.UnityAdsUseTestMode;
    bool USE_ADS => PlatformInterface.instance.config.UnityAdsUseAds;

#if UNITY_ADS
    void Start() {
        PlatformInterface.instance.logger.Log($"PlatformUnityAdsInit.Start() - gameID={GAME_ID}, testMode={USE_TEST_MODE}");
        if (USE_ADS) {
            //Advertisement.debugMode = true;
            // 개발 환경 머신에서 프록시 연결 설정 된 경우 아래 코드 처리 중 내부에서 UriFormatException 예외 발생한다.
            // 개발 중이니 가볍게 무시해도 될듯?
            try {
                Advertisement.Initialize(GAME_ID, USE_TEST_MODE);
            } catch (UriFormatException) {
                Debug.Log("Unity Ads exception due to proxy setting. Safely ignore this if you are a developer.");
            }
        }
    }

    void OnEnable() {
        Advertisement.AddListener(this);
    }

    void OnDisable() {
        Advertisement.RemoveListener(this);
    }

    public void OnUnityAdsDidError(string message) {
        UnityMainThreadDispatcher.Instance().Enqueue(OnUnityAdsDidErrorCoro(message));
    }

    IEnumerator OnUnityAdsDidErrorCoro(string message) {
        yield return null;
        Debug.LogError($"PlatformUnityAdsInit.OnUnityAdsDidError: message={message}");
        PlatformUnityAds.ShowAdsErrorPopup(message);
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult) {
        UnityMainThreadDispatcher.Instance().Enqueue(OnUnityAdsDidFinishCoro(placementId, showResult));
    }

    IEnumerator OnUnityAdsDidFinishCoro(string placementId, ShowResult showResult) {
        yield return null;
        PlatformInterface.instance.logger.Log($"PlatformUnityAdsInit.OnUnityAdsDidFinish: placementId={placementId}, showResult={showResult}");
        PlatformInterface.instance.backgroundTimeCompensator.EndBackgroundState(this);
        PlatformUnityAds.HandleShowResult(showResult);
    }

    public void OnUnityAdsDidStart(string placementId) {
        UnityMainThreadDispatcher.Instance().Enqueue(OnUnityAdsDidStartCoro(placementId));
    }

    IEnumerator OnUnityAdsDidStartCoro(string placementId) {
        yield return null;
        PlatformInterface.instance.logger.Log($"PlatformUnityAdsInit.OnUnityAdsDidStart: placementId={placementId}");
        PlatformInterface.instance.backgroundTimeCompensator.BeginBackgroundState(this);
    }

    public void OnUnityAdsReady(string placementId) {
        UnityMainThreadDispatcher.Instance().Enqueue(OnUnityAdsReadyCoro(placementId));
    }

    IEnumerator OnUnityAdsReadyCoro(string placementId) {
        yield return null;
        PlatformInterface.instance.logger.Log($"PlatformUnityAdsInit.OnUnityAdsReady: placementId={placementId}");
    }
#endif
}
