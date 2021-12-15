using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ConditionalDebug;
using JetBrains.Annotations;
using MiniJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Dict = System.Collections.Generic.Dictionary<string, object>;

[DisallowMultipleComponent]
public class ConfigPopup : MonoBehaviour
{
    public static ConfigPopup instance;

    public static readonly string BaseUrl =
        "https://xxxxx/xxxxxx/xxxxxx";

    static readonly string ServiceDbUrl = BaseUrl + "/service";

    public static string noticeDbPostfix = "";

    // BlackLanguageCode enum 순서 그대로 드롭다운에 있으리라는 보장은 없다.
    // 여길 수정하면 UpdateLanguageDropdownText()도 함께 바꿔줘야한다.
    static readonly List<BlackLanguageCode> languageDropdownValueArray = new List<BlackLanguageCode>
    {
        BlackLanguageCode.Ko,
        BlackLanguageCode.Ch,
        BlackLanguageCode.Tw,
        BlackLanguageCode.Ja,
        BlackLanguageCode.En
    };

    static readonly int Appear = Animator.StringToHash("Appear");

    [SerializeField]
    Toggle alwaysOnToggle;

    [SerializeField]
    Text appMetaInfoText;

    //외부 직접 접근이 필요한 변수/인스펙터 필드
    [SerializeField]
    Slider bgmSlider;

    [SerializeField]
    Toggle bgmToggle;

    [SerializeField]
    Toggle bigScreenToggle;

    [SerializeField]
    Toggle bottomNotchToggle;

    [SerializeField]
    GameObject communityConfigTabNewImage;

    [SerializeField]
    List<GameObject> configButtonGroupEtc;

    [SerializeField]
    Dropdown languageDropdown;

    [SerializeField]
    Button logoutButton;

    [SerializeField]
    GameObject mergeTab;

    [SerializeField]
    Toggle notchToggle;

    [SerializeField]
    GameObject noticeButtonNewImage;

    [SerializeField]
    Toggle performanceModeToggle;

    [SerializeField]
    Platform platform;

    [SerializeField]
    Slider sfxSlider;

    [SerializeField]
    Toggle sfxToggle;

    [SerializeField]
    Subcanvas subcanvas;

    Animator topAnimator;

    [SerializeField]
    Text userPseudoIdText;


    [SerializeField]
    [FormerlySerializedAs("VibrationGroup")]
    GameObject vibrationGroup; //진동 옵션 그룹

    static TopNotchOffsetGroup[] TopNotchOffsetGroupList => SingletonManager.instance.TopNotchOffsetGroupList;
    static BottomNotchOffsetGroup[] BottomNotchOffsetGroupList => SingletonManager.instance.BottomNotchOffsetGroupList;

    public bool IsNotchOn
    {
        get => notchToggle.isOn;
        set => notchToggle.isOn = value;
    }

    public bool IsBottomNotchOn
    {
        get => bottomNotchToggle.isOn;
        set => bottomNotchToggle.isOn = value;
    }

    public bool IsPerformanceModeOn
    {
        get => performanceModeToggle.isOn;
        set => performanceModeToggle.isOn = value;
    }

    public bool IsAlwaysOnOn
    {
        get => alwaysOnToggle.isOn;
        set => alwaysOnToggle.isOn = value;
    }

    public bool IsBigScreenOn
    {
        get => bigScreenToggle.isOn;
        set => bigScreenToggle.isOn = value;
    }


    public static string ServiceId =>
        $"{BlackContext.instance.UserPseudoId / 1000000:D3}-{BlackContext.instance.UserPseudoId / 1000 % 1000:D3}-{BlackContext.instance.UserPseudoId % 1000:D3}";

    public static string NoticeDbUrl => BaseUrl + "/notice" + noticeDbPostfix;
    static GameObject ConfigButtonNewImage => SingletonManager.instance.ConfigButtonNewImage;

    static bool EtcGroupVisible =>
        Application.systemLanguage == SystemLanguage.Korean || BlackContext.instance.CheatMode;

    bool IsOpen => subcanvas.IsOpen;

#if UNITY_EDITOR
    void OnValidate()
    {
        AutoBindUtil.BindAll(this);
    }
#endif

    void Awake()
    {
        topAnimator = GetComponentInParent<Animator>();

        if (BlackContext.instance.LoadedAtLeastOnce == false)
            // 고성능 모드 기본값은 OFF다. 저장 데이터 복원 이전에 호출되어 있어야 저장 데이터가 우선순위를 가진다.
            SetPerformanceMode(false);
        else
            Debug.LogError("Logic error: Initialized after loaded!");
    }

    void Start()
    {
        UpdateServiceText();
        UpdateLanguageDropdownText();
    }

    void Update()
    {
        if (IsOpen)
            if (logoutButton != null)
                if (logoutButton.gameObject.activeSelf)
                    logoutButton.interactable = PlatformLogin.IsAuthenticated;
    }

    [UsedImplicitly]
    void OpenPopup()
    {
        UpdateSoundToggleStates();
        UpdateSoundSliderStates();
        UpdateServiceText();
        UpdateEtcGroupVisibility();
        topAnimator.SetTrigger(Appear);
        logoutButton.gameObject.SetActive(Application.isEditor || Application.platform == RuntimePlatform.Android);
    }

    [UsedImplicitly]
    void ClosePopup()
    {
        // should receive message even if there is nothing to do
    }

    public static string GetAppMetaInfo()
    {
        var appMetaInfo = Resources.Load<AppMetaInfo>("App Meta Info");
        var platformVersionCode = "UNKNOWN";

        if (appMetaInfo != null)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    platformVersionCode = appMetaInfo.androidBundleVersionCode.ToString();
                    break;
                case RuntimePlatform.IPhonePlayer:
                    platformVersionCode = appMetaInfo.iosBuildNumber;
                    break;
                default:
                    platformVersionCode = $"{appMetaInfo.androidBundleVersionCode},{appMetaInfo.iosBuildNumber}";
                    break;
            }

            return
                $"v{Application.version}#{appMetaInfo.buildNumber} {appMetaInfo.buildStartDateTime} [{platformVersionCode}]" +
                (BlackContext.instance.CheatMode ? "/ADM" : "");
        }

        return $"v{Application.version} [{platformVersionCode}]" + (BlackContext.instance.CheatMode ? "/ADM" : "");
    }

    public static string GetUserId()
    {
        return $"ID: {ServiceId}-{BlackContext.instance.LastConsumedServiceIndex.ToInt():D3}";
    }

    void UpdateServiceText()
    {
        if (userPseudoIdText != null) userPseudoIdText.text = GetUserId();

        if (appMetaInfoText != null) appMetaInfoText.text = GetAppMetaInfo();
    }

    public void UpdateSoundToggleStates()
    {
        bgmToggle.isOn = Sound.instance.BgmAudioSourceActive;
        sfxToggle.isOn = Sound.instance.SfxAudioSourceActive;
    }

    void UpdateSoundSliderStates()
    {
        bgmSlider.value = Sound.instance.BgmAudioSourceVolume;
        sfxSlider.value = Sound.instance.SfxAudioSourceVolume;
    }

    public void StartCloudSave()
    {
        platform.CloudSave();
    }

    public void StartCloudLoad()
    {
        platform.CloudLoad();
    }

    static IEnumerator CheckAndReceiveServiceCoro()
    {
        ProgressMessage.instance.Open("\\서비스 항목 확인중...".Localized());
        var url = $"{ServiceDbUrl}/{ServiceId}";
        ConDebug.Log($"Querying {url}...");
        using var request = UnityWebRequest.Get(url);
        request.timeout = 5;
        yield return request.SendWebRequest();
        ProgressMessage.instance.Close();
        if (request.result == UnityWebRequest.Result.ConnectionError)
            ShortMessage.instance.Show("\\서비스 정보 수신에 실패했습니다.".Localized(), true);
        else
            try
            {
                var received = new List<string>();
                //ConDebug.LogFormat("URL Text: {0}", request.downloadHandler.text);
                var serviceDataRoot = Json.Deserialize(request.downloadHandler.text) as Dict;

                if (serviceDataRoot == null) yield break;

                foreach (var kv in serviceDataRoot) ConDebug.LogFormat("root key: {0}", kv.Key);

                var serviceData = serviceDataRoot["fields"] as Dictionary<string, object>;
                foreach (var kv in serviceData) ConDebug.LogFormat("fields key: {0}", kv.Key);

                //ConDebug.LogFormat("serviceData = {0}", serviceData);
                var serviceIndexList = new List<int>();
                foreach (var service in serviceData)
                {
                    var serviceIndexParsed = int.TryParse(service.Key, out var serviceIndex);
                    // 이미 받았거나 이상한 항목은 스킵
                    if (serviceIndexParsed == false
                        || serviceIndex <= BlackContext.instance.LastConsumedServiceIndex)
                        continue;

                    var fields = (Dict) ((Dict) ((Dict) service.Value)["mapValue"])["fields"];
                    //var serviceValue = service.Value as 
                    foreach (var serviceItem in fields)
                        switch (serviceItem.Key)
                        {
                            case "testitem":
                            {
                                break;
                            }
                        }

                    serviceIndexList.Add(serviceIndex);
                }

                if (serviceIndexList.Count > 0) BlackContext.instance.LastConsumedServiceIndex = serviceIndexList.Max();

                if (received.Count > 0)
                    ConfirmPopup.instance.Open(string.Format("\\다음 항목을 받았습니다.".Localized() + "\n\n{0}",
                        string.Join("\n", received.ToArray())));
                else
                    ShortMessage.instance.Show("\\모든 서비스 항목이 처리됐습니다.".Localized());
            }
            catch (Exception e)
            {
                ConfirmPopup.instance.Open("\\받은 서비스 항목이 없습니다.".Localized());
                Debug.LogWarning(e.ToString());
            }
    }

    internal void ActivateNoticeNewImage(bool b)
    {
        if (EtcGroupVisible)
        {
            ConfigButtonNewImage.SetActive(b);
            communityConfigTabNewImage.SetActive(b);
            noticeButtonNewImage.SetActive(b);
        }
        else
        {
            ConfigButtonNewImage.SetActive(false);
            communityConfigTabNewImage.SetActive(false);
            noticeButtonNewImage.SetActive(false);
        }
    }

    public void CheckAndReceiveService()
    {
        BlackLogManager.Add(BlackLogEntry.Type.GameOpenService, 0, 0);
        StartCoroutine(CheckAndReceiveServiceCoro());
    }

    // should only called by Toggle component event callback
    public void EnableGatherStoredMaxSfx(bool b)
    {
        if (Sound.instance != null) Sound.instance.GatherStoredMaxSfxEnabled = b;
    }

    // should only called by Toggle component event callback
    public void EnableNotchSupport(bool b)
    {
        foreach (var topNotchOffsetGroup in TopNotchOffsetGroupList)
            if (topNotchOffsetGroup != null)
                topNotchOffsetGroup.NotchMarginActive = b;
    }

    // should only called by Toggle component event callback
    public void EnableBottomNotchSupport(bool b)
    {
        foreach (var bottomNotchOffsetGroup in BottomNotchOffsetGroupList)
            if (bottomNotchOffsetGroup != null)
                bottomNotchOffsetGroup.NotchMarginActive = b;
    }

    public static void SetPerformanceMode(bool b)
    {
        if (b)
        {
            QualitySettings.vSyncCount = 0;
            if (Application.isEditor)
                Application.targetFrameRate = -1;
            else
                Application.targetFrameRate = 60;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 30;
        }

        ConDebug.Log($"Performance Mode set to {b}");
    }

    static void SetAlwaysOn(bool b)
    {
        Screen.sleepTimeout = b ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
        ConDebug.Log($"Always On set to {b}");
    }

    // should only called by Toggle component event callback
    public void EnablePerformanceMode(bool b)
    {
        SetPerformanceMode(b);
    }

    // should only called by Toggle component event callback
    public void EnableAlwaysOn(bool b)
    {
        SetAlwaysOn(b);
    }

    static int GetLanguageIndex(BlackLanguageCode languageCode)
    {
        return languageDropdownValueArray.FindIndex(e => e == languageCode);
    }

    public void EnableLanguage(BlackLanguageCode languageCode)
    {
        if (languageDropdown != null) languageDropdown.value = GetLanguageIndex(languageCode);
    }

    // Dropdown 콜백으로만 호출되어야 한다.
    // 직접 호출하면 안된다.
    public void OnLanguageValueChanged(int languageIndex)
    {
        ConDebug.Log($"Language selected: {languageIndex}");
        BlackContext.instance.RefreshRiceText();
        BlackContext.instance.RefreshGemText();
        languageDropdown.RefreshShownValue();
        ChangeLanguage(gameObject.scene, languageDropdownValueArray[languageIndex]);
    }

    public static void ChangeLanguage(Scene scene, BlackLanguageCode languageCode)
    {
        ConDebug.Log($"ChangeLanguage: from {Data.instance.CurrentLanguageCode} to {languageCode}...");
        if (Data.instance.CurrentLanguageCode != languageCode)
        {
            Data.instance.CurrentLanguageCode = languageCode;
            UpdateAllTexts(scene);
            UpdateLanguageDropdownText();
            ConDebug.Log($"ChangeLanguage: changed to {Data.instance.CurrentLanguageCode}");
        }
        else
        {
            ConDebug.Log($"ChangeLanguage: already {languageCode}");
        }
    }

    static void UpdateLanguageDropdownText()
    {
        if (instance.languageDropdown == null) return;

        if (instance != null)
        {
            var languageOptions = new List<string>
            {
                "\\한국어".Localized(),
                "\\중국어 (간체)".Localized(),
                "\\중국어 (번체)".Localized(),
                "\\일본어".Localized(),
                "\\영어".Localized()
            };
            for (var i = 0; i < languageOptions.Count; i++)
                instance.languageDropdown.options[i].text = languageOptions[i];

            instance.languageDropdown.RefreshShownValue();
        }
    }

    static void UpdateAllTexts(Scene scene)
    {
        // Debug.LogError("not expected to be called");
        foreach (var root in scene.GetRootGameObjects())
        {
            var languageFont = FontManager.instance.GetLanguageFont(Data.instance.CurrentLanguageCode);
            foreach (var textCollector in root.GetComponentsInChildren<TextCollector>(true))
            {
                foreach (var text in textCollector.AllTextsInPrefab) text.font = languageFont;

                foreach (var staticLocalizedText in textCollector.AllStaticLocalizedTextsInPrefab)
                    staticLocalizedText.UpdateText();
            }

            foreach (var fitter in root.GetComponentsInChildren<ContentSizeFitter>())
            {
                if (!fitter.enabled) continue;

                fitter.enabled = false;
                // ReSharper disable once Unity.InefficientPropertyAccess
                fitter.enabled = true;
            }

            if (BlackContext.instance != null)
            {
                BlackContext.instance.RefreshRiceText();
                BlackContext.instance.RefreshGemText();
            }
        }
    }

    void UpdateEtcGroupVisibility()
    {
        // 시스템 언어가 한국어인 경우 혹은 치트 모드에서만 기타 그룹 버튼(응급 서비스 확인, 카페 가기, ...) 보여준다.
        foreach (var btn in configButtonGroupEtc) btn.gameObject.SetActive(EtcGroupVisible);
    }

    public void OpenCommunity()
    {
        Application.OpenURL("https://cafe.naver.com/blacktycoon");
    }

    public void RequestUserReview()
    {
        Platform.instance.RequestUserReview();
    }

    public void OpenNotice()
    {
        if (NoticeManager.instance != null) NoticeManager.instance.Open();
    }

    public void ReportSaveData()
    {
        if (Admin.instance != null) Admin.instance.ReportSaveData();
    }

    public void ReportPlayLog()
    {
        if (Admin.instance != null) Admin.instance.ReportPlayLog();
    }

    public void OpenPrivacyPolicy()
    {
        Application.OpenURL("http://privacy.plusalpha.top");
    }

    public void CloseSubcanvas()
    {
        subcanvas.Close();
    }

    public void PlayButtonClick()
    {
        PlayButtonClickOnlyIfTrue(true);
    }

    public void PlayButtonClickOnlyIfTrue(bool b)
    {
        if (b && Sound.instance != null) Sound.instance.PlayButtonClick();
    }

    public void BgmAudioSourceActive(bool b)
    {
        if (Sound.instance != null) Sound.instance.BgmAudioSourceActive = b;
    }

    public void SfxAudioSourceActive(bool b)
    {
        if (Sound.instance != null) Sound.instance.SfxAudioSourceActive = b;
    }

    [UsedImplicitly]
    public void BgmAudioSourceVolume(float v)
    {
        if (Sound.instance != null) Sound.instance.BgmAudioSourceVolume = v;
    }

    [UsedImplicitly]
    public void SfxAudioSourceVolume(float v)
    {
        if (Sound.instance != null) Sound.instance.SfxAudioSourceVolume = v;
    }

    public void Logout()
    {
        Platform.instance.Logout();
    }
}