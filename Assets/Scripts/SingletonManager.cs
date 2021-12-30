using UnityEngine;

[DisallowMultipleComponent]
public class SingletonManager : MonoBehaviour
{
    public static SingletonManager instance;

    [SerializeField]
    AchievementPopup achievementPopup;

    [SerializeField]
    AchievePopup achievePopup;

    [SerializeField]
    Admin admin;

    [SerializeField]
    BackButtonHandler backButtonHandler;

    [SerializeField]
    BackgroundTimeCompensator backgroundTimeCompensator;

    [SerializeField]
    BlackContext blackContext;

    [SerializeField]
    BlackPlatform blackPlatform;

    [SerializeField]
    BottomNotchOffsetGroup[] bottomNotchOffsetGroupList;

    [SerializeField]
    GameObject configButtonNewImage;

    [SerializeField]
    ConfigPopup configPopup;

    [SerializeField]
    ConfirmPopup confirmPopup;

    [SerializeField]
    Data data;

    [SerializeField]
    ErrorReporter errorReporter;

    [SerializeField]
    FontManager fontManager;

    [SerializeField]
    NoticeManager noticeManager;

    [SerializeField]
    PlatformAdMobAds platformAdMobAds;

    [SerializeField]
    PlatformAdMobAdsInit platformAdMobAdsInit;

    [SerializeField]
    PlatformInterface platformInterface;

    [SerializeField]
    PlatformReceiptVerifier platformReceiptVerifier;

    [SerializeField]
    ProgressMessage progressMessage;

    [SerializeField]
    SaveLoadManager saveLoadManager;

    [SerializeField]
    ShortMessage shortMessage;

    [SerializeField]
    SocialScoreReporter socialScoreReporter;

    [SerializeField]
    Sound sound;

    [SerializeField]
    TopNotchOffsetGroup[] topNotchOffsetGroupList;

    public GameObject ConfigButtonNewImage => configButtonNewImage;
    public TopNotchOffsetGroup[] TopNotchOffsetGroupList => topNotchOffsetGroupList;
    public BottomNotchOffsetGroup[] BottomNotchOffsetGroupList => bottomNotchOffsetGroupList;

    void Awake()
    {
        instance = this;

        // Data.instance가 먼저 갱신되어야 한다.
        // Data.instance에 의존하는 싱글턴이 많기 때문
        // 예를 들어, Data.instance가 예전 것인데, ContestBlackContext.Awake()가 먼저 호출되면
        // 언어 변경이 제대로 작동하지 않는다. (이미 언어 변경이 En으로 끝난 예전 Data.instance를 참고할 것이므로)
        // 그래서 Data.instance를 가장 먼저 바꾼다.
        Data.instance = data;
        ConfirmPopup.instance = confirmPopup;
        ProgressMessage.instance = progressMessage;
        Sound.instance = sound;
        AchievementPopup.instance = achievementPopup;
        AchievePopup.instance = achievePopup;
        ShortMessage.instance = shortMessage;
        BlackContext.instance = blackContext;
        ConfigPopup.instance = configPopup;
        NoticeManager.instance = noticeManager;
        ErrorReporter.instance = errorReporter;
        FontManager.instance = fontManager;
        SocialScoreReporter.instance = socialScoreReporter;
        BackButtonHandler.instance = backButtonHandler;
        Admin.instance = admin;
        SaveLoadManager.instance = saveLoadManager;
        BackgroundTimeCompensator.instance = backgroundTimeCompensator;

        // 아주 깔끔한 구조는 아니지만, 최대한 기존 코드 안수정하고 하려니까 이렇게 됐다.
        BlackPlatform.instance = blackPlatform;

        PlatformInterface.instance = platformInterface;

        if (PlatformInterface.instance != null)
        {
            PlatformInterface.instance.saveLoadManager = saveLoadManager;
            // BlackLogManager는 SingletonManager보다 우선순위 높게 초기화되므로 이렇게 써도 된다.
            PlatformInterface.instance.logManager = BlackLogManager.instance;
            PlatformInterface.instance.logEntryType = new BlackLogEntry();
            PlatformInterface.instance.textHelper = new TextHelper();
            PlatformInterface.instance.confirmPopup = ConfirmPopup.instance;
            PlatformInterface.instance.progressMessage = ProgressMessage.instance;
            PlatformInterface.instance.saveUtil = BlackPlatform.instance;
            PlatformInterface.instance.notification = new BlackNotification();
            PlatformInterface.instance.text = BlackPlatform.instance;
            PlatformInterface.instance.ads = new PlatformAds();
            PlatformInterface.instance.shortMessage = ShortMessage.instance;
            PlatformInterface.instance.config = BlackPlatform.instance;
            PlatformInterface.instance.logger = new PlatformLogger();
        }

#if GOOGLE_MOBILE_ADS
        PlatformAdMobAdsInit.instance = platformAdMobAdsInit;
        PlatformAdMobAds.instance = platformAdMobAds;
#endif
        PlatformReceiptVerifier.instance = platformReceiptVerifier;
    }
}