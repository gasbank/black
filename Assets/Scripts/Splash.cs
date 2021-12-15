using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Splash : MonoBehaviour
{
    [SerializeField]
    Slider slider;

    [SerializeField]
    Sprite[] loadingSpriteList;

    [SerializeField]
    Image loadingImage;

    [SerializeField]
    GameObject startButton;

    [SerializeField]
    GameObject reportSaveDataButton;

    [SerializeField]
    bool reportSaveDataBuild;

    static int splashStartCount;

    static readonly string SplashSceneName = "Splash";

#if UNITY_ANDROID || UNITY_IOS
// 해상도 조절 없다.
#elif UNITY_WEBGL
// 어떻게 해야할까...?
#else
    void Awake()
    {
        // 스크린샷 찍기 용도의 해상도로 강제 지정
        Screen.SetResolution(1080, 1920, FullScreenMode.Windowed, 60);
    }
#endif

    IEnumerator Start()
    {
        if (reportSaveDataBuild)
        {
            loadingImage.gameObject.SetActive(false);
            slider.gameObject.SetActive(false);
            reportSaveDataButton.SetActive(true);
        }
        else if (Debug.isDebugBuild)
        {
            startButton.SetActive(true);
        }
        else
        {
            yield return StartLoadCoro();
        }
    }

    public void StartLoad()
    {
        startButton.SetActive(false);
        StartCoroutine(StartLoadCoro());
    }

    IEnumerator StartLoadCoro()
    {
        // 에디터에서만 로그를 보자
        // 원래는 SingletonManager.cs에서 로그를 끄는 작업을 했으나,
        // 이렇게 끄면 Main 씬이 리로드 될 때마다 로그가 꺼져서
        // 세이브 데이터 로드 등 오류가 발생했을 때 문제 상황을 확인하기가 무척 곤란다.
        // 실행할 때만 한번 끄고, 이후 어드민 커맨드 혹은 로그 켜기 커맨드만으로 로그가 활성화되면
        // Main 씬이 리로드 되더라도 로그가 나올 수 있도록 한다.
        // 클라우드 로드, 어드민 커맨드로 세이브 데이터 리셋 시에도 다시 여기로 오게 되는데,
        // 두 번째 부터는 로그 플래그를 만지지 않아야, 세이브 데이터 관련 디버깅 로그를 보는 것이
        // 편리하다.
        if (splashStartCount == 0)
        {
#if BLACK_ADMIN
            Debug.unityLogger.logEnabled = true;
#else
            Debug.unityLogger.logEnabled = Application.isEditor;
#endif
        }

        splashStartCount++;

        var loadSceneMode = LoadSceneMode.Single;

        var loadingAsync = SceneManager.LoadSceneAsync("Main", loadSceneMode);
        loadingAsync.allowSceneActivation = false;
        while (loadingAsync.progress < 0.9f)
        {
            slider.normalizedValue = loadingAsync.progress;
            yield return null;
        }

        slider.normalizedValue = 1.0f;
        loadingAsync.allowSceneActivation = true;
        while (!loadingAsync.isDone) yield return loadingAsync;
    }

    public static void LoadSplashScene()
    {
        SceneManager.LoadScene(SplashSceneName);
    }

    public static void LoadSplashSceneAdditive()
    {
        SceneManager.LoadScene(SplashSceneName, LoadSceneMode.Additive);
    }
}