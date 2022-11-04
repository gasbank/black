using UnityEngine;

[DisallowMultipleComponent]
public class SingletonManager : MonoBehaviour
{
    public static SingletonManager Instance;

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
    ToastMessage toastMessage;

    [SerializeField]
    TopNotchOffsetGroup[] topNotchOffsetGroupList;

    [SerializeField]
    IntroDirector introDirector;

    [SerializeField]
    MainGame mainGame;

    [SerializeField]
    CreditPopup creditPopup;

    public GameObject ConfigButtonNewImage => configButtonNewImage;
    public TopNotchOffsetGroup[] TopNotchOffsetGroupList => topNotchOffsetGroupList;
    public BottomNotchOffsetGroup[] BottomNotchOffsetGroupList => bottomNotchOffsetGroupList;

    void Awake()
    {
        Instance = this;

        // Data.instance가 먼저 갱신되어야 한다.
        // Data.instance에 의존하는 싱글턴이 많기 때문
        // 예를 들어, Data.instance가 예전 것인데, ContestBlackContext.Awake()가 먼저 호출되면
        // 언어 변경이 제대로 작동하지 않는다. (이미 언어 변경이 En으로 끝난 예전 Data.instance를 참고할 것이므로)
        // 그래서 Data.instance를 가장 먼저 바꾼다.
        Data.Instance = data;
        ConfirmPopup.Instance = confirmPopup;
        ProgressMessage.Instance = progressMessage;
        Sound.Instance = sound;
        AchievementPopup.Instance = achievementPopup;
        AchievePopup.Instance = achievePopup;
        ShortMessage.Instance = shortMessage;
        BlackContext.Instance = blackContext;
        ConfigPopup.Instance = configPopup;
        NoticeManager.Instance = noticeManager;
        ErrorReporter.Instance = errorReporter;
        FontManager.Instance = fontManager;
        SocialScoreReporter.Instance = socialScoreReporter;
        BackButtonHandler.Instance = backButtonHandler;
        Admin.Instance = admin;
        SaveLoadManager.Instance = saveLoadManager;
        BackgroundTimeCompensator.Instance = backgroundTimeCompensator;
        ToastMessage.Instance = toastMessage;
        IntroDirector.Instance = introDirector;
        MainGame.Instance = mainGame;
        CreditPopup.Instance = creditPopup;

        // 아주 깔끔한 구조는 아니지만, 최대한 기존 코드 안수정하고 하려니까 이렇게 됐다.
        BlackPlatform.Instance = blackPlatform;

        PlatformInterface.Instance = platformInterface;

        if (PlatformInterface.Instance != null)
        {
            PlatformInterface.Instance.saveLoadManager = saveLoadManager;
            // BlackLogManager는 SingletonManager보다 우선순위 높게 초기화되므로 이렇게 써도 된다.
            PlatformInterface.Instance.logManager = BlackLogManager.Instance;
            PlatformInterface.Instance.logEntryType = new BlackLogEntry();
            PlatformInterface.Instance.textHelper = new TextHelper();
            PlatformInterface.Instance.confirmPopup = ConfirmPopup.Instance;
            PlatformInterface.Instance.progressMessage = ProgressMessage.Instance;
            PlatformInterface.Instance.saveUtil = BlackPlatform.Instance;
            PlatformInterface.Instance.notification = new BlackNotification();
            PlatformInterface.Instance.text = BlackPlatform.Instance;
            PlatformInterface.Instance.ads = new PlatformAds();
            PlatformInterface.Instance.shortMessage = ShortMessage.Instance;
            PlatformInterface.Instance.config = BlackPlatform.Instance;
            PlatformInterface.Instance.logger = new PlatformLogger();
        }

#if GOOGLE_MOBILE_ADS
        PlatformAdMobAdsInit.Instance = platformAdMobAdsInit;
        PlatformAdMobAds.Instance = platformAdMobAds;
#endif
        PlatformReceiptVerifier.Instance = platformReceiptVerifier;
    }
}