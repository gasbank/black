﻿using System;
using System.Collections;
using ConditionalDebug;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Subcanvas), typeof(RectTransform))]
public class ConfirmPopup : MonoBehaviour, IPlatformConfirmPopup
{
    public static IPlatformConfirmPopup Instance;

    static readonly int Appear = Animator.StringToHash("Appear");

    [SerializeField]
    int autoCloseRemainSec = -1;

    [SerializeField]
    Text autoCloseText;

    public Button button1;
    public Text button1Text;
    public Button button2;
    public Text button2Text;
    public Button claimButton;
    public Text claimButtonText;
    public Image claimImage;
    public Button closeButton;

    [SerializeField]
    RectTransform contentTopRect;

    [SerializeField]
    GameObject customSpawnAtOnceButtonGroup;

    // 제목 타입 3
    public GameObject decorated2Header;

    public Text decorated2Title;

    // 제목 타입 4
    public GameObject decorated3Header;

    public Text decorated3Title;

    // 제목 타입 2
    public GameObject decoratedHeader;

    public Text decoratedTitle;

    [SerializeField]
    GameObject gatherButtonPowerDownToggleGroup;

    [SerializeField]
    Sprite gemClaimButtonImageSprite;

    [SerializeField]
    Sprite gemClaimImageSprite;

    [SerializeField]
    InputField inputField;

    [SerializeField]
    Text inputFieldPlaceholderText;

    // 본문 표시부 -------
    public Text message;
    Action onButton1;
    Action onButton2;
    Action onClaimButton;
    Action onCloseButton;

    public Image popupImage;
    public LayoutElement popupImageLayoutElement;

    [SerializeField]
    RectTransform popupImageParentRect;

    [SerializeField]
    Sprite goldClaimButtonImageSprite;

    [SerializeField]
    Sprite goldClaimImageSprite;

    [SerializeField]
    Graphic skipRenderGraphic;

    [SerializeField]
    Subcanvas subcanvas;

    // 제목 표시부 ------
    // 제목 타입 1
    public Text title;

    [SerializeField]
    VerticalLayoutGroup titleGroup;

    [SerializeField]
    Animator topAnimator;

    [SerializeField]
    Image topImage;

    int AutoCloseRemainSec
    {
        get => autoCloseRemainSec;
        set
        {
            autoCloseRemainSec = value;
            if (autoCloseText != null)
            {
                autoCloseText.gameObject.SetActive(autoCloseRemainSec > 0);
                if (autoCloseRemainSec > 0) autoCloseText.text = "\\{0}초후 자동으로 닫힙니다.".Localized(autoCloseRemainSec);
            }
        }
    }

    public bool IsOpen => subcanvas.IsOpen;

    public string InputFieldText => inputField.text;

    void Awake()
    {
        // 에디터에서 열어둔 채로 프리팹 저장하는 경우가 허다하다. 끄고 시작하자.
        subcanvas.Close();
    }

    public void OpenYesNoPopup(string msg, Action onYes, Action onNo)
    {
        OpenYesNoPopup(msg, onYes, onNo, "\\확인".Localized());
    }

    void IPlatformConfirmPopup.Open(string msg)
    {
        Open(msg);
    }

    public void OpenConfirmPopup(string msg, Action onBtn1, Action onBtn2, Action onBtn3,
        string titleText, Header header, string btn1Text, string btn2Text, string btn3Text,
        string inputFieldText = "", string inputFieldPlaceholder = "", bool showInputField = false,
        Sprite popupSprite = null, Sprite topImageSprite = null,
        WidthType widthType = WidthType.Normal,
        float popupImageTopOffset = 0,
        ShowPosition showPosition = ShowPosition.Center,
        Action onCloseBtn = null,
        bool allowTouchEventsOutsidePopup = false,
        int autoCloseSec = -1,
        Font titleMessageFont = null,
        Sprite claimImageSprite = null,
        Sprite claimBackgroundImageSprite = null,
        bool showGatherButtonConfigGroup = false,
        bool showCoreIndexSelection = false,
        CoreIndexSelectionMode coreIndexSelectionMode = CoreIndexSelectionMode.DontCare,
        Action<int> coreIndexSelectionCallback = null)
    {
        if (contentTopRect != null)
        {
            // 너비가 넓은 버전(기본 버전) 팝업인지, 좁은 버전인지
            contentTopRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (float) widthType);

            // 화면 가운데에 나오게 할지 아니면 하단에 나오게 할지
            if (showPosition == ShowPosition.Center)
                contentTopRect.anchorMin = contentTopRect.anchorMax = contentTopRect.pivot = new Vector2(0.5f, 0.5f);
            else
                contentTopRect.anchorMin = contentTopRect.anchorMax = contentTopRect.pivot = new Vector2(0.5f, 0.0f);
        }

        // 팝업 오른쪽 위에 X 표시 버튼 표시할지 말지
        if (closeButton != null)
        {
            closeButton.gameObject.SetActive(onCloseBtn != null);
            onCloseButton = onCloseBtn;
        }

        // 팝업 뒤에 있는 요소들로 터치 이벤트를 허용할지 말지 결정한다.
        // skipRenderGraphic이 켜져 있으면 이벤트를 허용하지 않는다.
        if (skipRenderGraphic != null) skipRenderGraphic.enabled = allowTouchEventsOutsidePopup == false;

        // top image
        if (topImage != null)
        {
            topImage.gameObject.SetActive(topImageSprite != null);
            topImage.sprite = topImageSprite;
        }

        UpdateTitle(titleText, header);
        if (message != null)
        {
            message.text = msg;
            message.gameObject.SetActive(string.IsNullOrEmpty(msg) == false);
        }

        if (title != null && message != null)
        {
            if (titleMessageFont != null)
                title.font = message.font = titleMessageFont;
            else
                title.font = message.font = FontManager.Instance.GetLanguageFont(Data.Instance.CurrentLanguageCode);
        }

        //image
        if (popupSprite != null)
        {
            var popupImageHeight = 256;
            ActivatePopupImage(popupSprite, popupImageTopOffset, popupImageHeight);
        }
        else
        {
            if (popupImageParentRect != null) popupImageParentRect.gameObject.SetActive(false);
        }

        // Button 1
        if (button1 != null)
        {
            if (onBtn1 != null)
            {
                button1.gameObject.SetActive(true);
                button1Text.text = btn1Text;
            }
            else
            {
                button1.gameObject.SetActive(false);
            }

            onButton1 = onBtn1;
        }

        // Button 2
        if (button2 != null)
        {
            if (onBtn2 != null)
            {
                button2.gameObject.SetActive(true);
                button2Text.text = btn2Text;
            }
            else
            {
                button2.gameObject.SetActive(false);
            }

            onButton2 = onBtn2;
        }

        // Button 3
        if (claimButton != null)
        {
            if (onBtn3 != null)
            {
                claimButton.gameObject.SetActive(true);
                claimButtonText.text = btn3Text;
                claimImage.sprite = claimImageSprite != null ? claimImageSprite : goldClaimImageSprite;
                claimButton.image.sprite = claimBackgroundImageSprite != null
                    ? claimBackgroundImageSprite
                    : goldClaimButtonImageSprite;
            }
            else
            {
                claimButton.gameObject.SetActive(false);
            }
        }

        onClaimButton = onBtn3;

        if (inputField != null)
        {
            inputField.gameObject.SetActive(showInputField);
            inputField.text = inputFieldText;
            inputFieldPlaceholderText.text = inputFieldPlaceholder;
        }

        AutoCloseRemainSec = autoCloseSec;

        if (gatherButtonPowerDownToggleGroup != null)
            gatherButtonPowerDownToggleGroup.gameObject.SetActive(showGatherButtonConfigGroup);

        subcanvas.Open();
    }

    public void ActivatePopupImage(Sprite popupSprite, float popupImageTopOffset, int popupImageHeight)
    {
        popupImageParentRect.gameObject.SetActive(true);
        popupImage.sprite = popupSprite;
        //popupImage.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, popupImageTopOffset, popupImageParentRect.rect.height - popupImageTopOffset);
        popupImage.rectTransform.anchorMin = Vector2.zero;
        popupImage.rectTransform.anchorMax = Vector2.one;
        popupImage.rectTransform.anchoredPosition = Vector2.zero; //popupImageParentRect.sizeDelta / 2;
        popupImage.rectTransform.pivot = new Vector2(0.5f, 0.0f); // lower middle
        popupImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
            popupImageParentRect.rect.size.x); // .sizeDelta;
        popupImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
            popupImageParentRect.rect.size.y + popupImageTopOffset); // .sizeDelta;
        SetPopupImageHeight(popupImageHeight);
    }

    public void OpenTwoButtonPopup(string msg, Action onBtn1, Action onBtn2, string titleText,
        string btn1Text, string btn2Text, Header header = Header.Normal,
        ShowPosition showPosition = ShowPosition.Center, Action onCloseBtn = null,
        bool allowTouchEventsOutsidePopup = false)
    {
        OpenConfirmPopup(msg, onBtn1, onBtn2, null, titleText, header, btn1Text, btn2Text, "", "", "", false, null, null,
            WidthType.Normal, 0, showPosition, onCloseBtn, allowTouchEventsOutsidePopup);
    }

    public void OpenInputFieldPopup(string msg, Action onYes, Action onNo, string titleText, Header header,
        string inputFieldText, string inputFieldPlaceholder,
        InputField.CharacterValidation validation = InputField.CharacterValidation.None)
    {
        OpenConfirmPopup(msg, onYes, onNo, null, titleText, header, "\\확인".Localized(), "\\취소".Localized(), "", inputFieldText,
            inputFieldPlaceholder, true);
        inputField.Select();
        inputField.ActivateInputField();
        inputField.characterValidation = validation;
        // 터치스크린 키보드 위의 입력창을 숨긴다. (게임 화면에도 입력창이 나오므로)
        inputField.shouldHideMobileInput = true;
        inputField.keyboardType = validation == InputField.CharacterValidation.Integer
            ? TouchScreenKeyboardType.NumberPad
            : TouchScreenKeyboardType.Default;
    }

    public void OpenGoldClaimPopup(string msg, string claimBtnText, Action onClaim, string titleText,
        Header header = Header.Normal, int autoCloseSec = -1)
    {
        OpenConfirmPopup(msg, null, null, onClaim, titleText, header, "", "", claimBtnText, "", "", false, null, null,
            WidthType.Normal, 0, ShowPosition.Center, null, false, autoCloseSec, null, goldClaimImageSprite,
            goldClaimButtonImageSprite);
    }

    public void OpenGemClaimPopup(string msg, string claimBtnText, Action onClaim, string titleText,
        Header header = Header.Normal, int autoCloseSec = -1)
    {
        OpenConfirmPopup(msg, null, null, onClaim, titleText, header, "", "", claimBtnText, "", "", false, null, null,
            WidthType.Normal, 0, ShowPosition.Center, null, false, autoCloseSec, null, gemClaimImageSprite,
            gemClaimButtonImageSprite);
    }

    public void Open(string msg, Action onYes = null)
    {
        Open(msg, onYes ?? Close, "\\확인".Localized());
    }

    public void Open(string msg, Action onYes, string titleText, Header header = Header.Normal)
    {
        OpenConfirmPopup(msg, onYes, null, null, titleText, header, "\\확인".Localized(), "", "");
    }

    public void OpenOneButtonPopup(string msg, Action onYes, string titleText, string confirmText,
        Header header = Header.Normal)
    {
        OpenConfirmPopup(msg, onYes, null, null, titleText, header, confirmText, "", "");
    }

    public void OpenYesImagePopup(string titleText, string msg, Action onYes, Sprite sprite)
    {
        OpenYesImagePopup(titleText, sprite, msg, "\\확인".Localized(), onYes);
    }

    public void OpenYesNoImagePopup(string textTitle, string msg, Action onYes, Sprite sprite)
    {
        OpenYesNoImagePopup(textTitle, sprite, msg, "\\받기".Localized(), "\\취소".Localized(), onYes, Close);
    }

    public void OpenYesNoImagePopup(string titleText, Sprite sprite, string msg, string btn1Text, string btn2Text,
        Action onBtn1, Action onBtn2)
    {
        OpenConfirmPopup(msg, onBtn1, onBtn2, null, titleText, Header.Normal, btn1Text, btn2Text, "", "", "", false, sprite);
    }

    public void Close()
    {
        if (IsOpen)
        {
            //gameObject.SetActive(false);
            subcanvas.Close();
        }
    }
    
    void IPlatformConfirmPopup.OpenTwoButtonPopup_Update(string text, Action close, Action action)
    {
        BlackPlatform.OpenTwoButtonPopup_Update(text, close, action);
    }

    public void ActivateTopImage(Sprite sprite)
    {
        topImage.gameObject.SetActive(true);
        topImage.sprite = sprite;
    }

    public GameObject ClaimImageGameObject => claimImage.gameObject;

    public void OpenCoreIndexSelectionPopup(string msg, string titleText, string btn1Text, Action<int> buttonCallback)
    {
        OpenConfirmPopup(msg, Close, null, null, titleText, Header.Normal, btn1Text, null, "", "", "", false, null, null,
            WidthType.Normal, 0, ShowPosition.Center, Close, false, -1, null, null, null, false, true,
            CoreIndexSelectionMode.SimpleButton, buttonCallback);
    }

    public string MessageText
    {
        get => message.text;
        set => message.text = value;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        subcanvas = GetComponent<Subcanvas>();
    }
#endif

    IEnumerator Start()
    {
        while (true)
        {
            //ConDebug.Log("ConfirmPopup.Start coro...");
            yield return new WaitForSeconds(1);
            if (AutoCloseRemainSec <= 0 || !IsOpen) continue;

            ConDebug.Log("ConfirmPopup.Start autoCloseRemainSec - 1...");
            AutoCloseRemainSec--;
            if (AutoCloseRemainSec != 0) continue;

            ConDebug.Log("ConfirmPopup.Start autoCloseRemainSec == 0");
            if (onCloseButton != null)
            {
                ConDebug.Log("ConfirmPopup.Start onClose");
                onCloseButton();
            }
            else if (onButton1 != null)
            {
                ConDebug.Log("ConfirmPopup.Start onButton1");
                onButton1();
            }
            else if (onButton2 != null)
            {
                ConDebug.Log("ConfirmPopup.Start onButton2");
                onButton2();
            }
            else if (onClaimButton != null)
            {
                ConDebug.Log("ConfirmPopup.Start onClaimButton");
                onClaimButton();
            }
        }

        // ReSharper disable once IteratorNeverReturns
    }

    [UsedImplicitly]
    void OpenPopup()
    {
        //topAnimator.SetTrigger(Appear);
        subcanvas.Open();
    }

    [UsedImplicitly]
    void ClosePopup()
    {
    }

    [UsedImplicitly]
    void ExecuteClosePopupDefaultAction()
    {
        if (IsOpen)
        {
            if (onClaimButton != null)
                onClaimButton();
            else if (onCloseButton != null)
                onCloseButton();
            else if (onButton2 != null)
                onButton2();
            else if (onButton1 != null)
                onButton1();
            else
                Debug.LogError("Could not handle back button!");
        }
    }

    void UpdateTitle(string titleText, Header header)
    {
        if (title == null) return;

        switch (header)
        {
            case Header.Decorated:
                decoratedTitle.text = titleText;
                if (titleGroup != null)
                {
                    titleGroup.padding.top = 0;
                }

                title.gameObject.SetActive(false);
                if (decoratedHeader != null)
                {
                    decoratedHeader.SetActive(true);
                }

                if (decorated2Header != null)
                {
                    decorated2Header.SetActive(false);
                }

                if (decorated3Header != null)
                {
                    decorated3Header.SetActive(false);
                }

                break;
            case Header.Decorated2:
                decorated2Title.text = titleText;
                if (titleGroup != null)
                {
                    titleGroup.padding.top = -33;
                }

                title.gameObject.SetActive(false);
                if (decoratedHeader != null)
                {
                    decoratedHeader.SetActive(false);
                }

                if (decorated2Header != null)
                {
                    decorated2Header.SetActive(true);
                }

                if (decorated3Header != null)
                {
                    decorated3Header.SetActive(false);
                }

                break;
            case Header.Decorated3:
                decorated3Title.text = titleText;
                if (titleGroup != null)
                {
                    titleGroup.padding.top = -33;
                }

                title.gameObject.SetActive(false);
                if (decoratedHeader != null)
                {
                    decoratedHeader.SetActive(false);
                }

                if (decorated2Header != null)
                {
                    decorated2Header.SetActive(false);
                }

                if (decorated3Header != null)
                {
                    decorated3Header.SetActive(true);
                }

                break;
            default:
                title.text = titleText;
                if (titleGroup != null)
                {
                    titleGroup.padding.top = 0;
                }

                title.gameObject.SetActive(true);
                if (decoratedHeader != null)
                {
                    decoratedHeader.SetActive(false);
                }

                if (decorated2Header != null)
                {
                    decorated2Header.SetActive(false);
                }

                if (decorated3Header != null)
                {
                    decorated3Header.SetActive(false);
                }

                break;
        }
    }

    public void OpenCustomSpawnAtOncePopup()
    {
        throw new NotImplementedException();
    }

    void SetPopupImageHeight(int popupImageHeight)
    {
        popupImageLayoutElement.flexibleHeight = popupImageLayoutElement.minHeight =
            popupImageLayoutElement.preferredHeight = popupImageHeight;
    }

    void OpenYesNoPopup(string msg, Action onYes, Action onNo, string titleText,
        Header header = Header.Normal)
    {
        OpenTwoButtonPopup(msg, onYes, onNo, titleText, "\\예".Localized(), "\\아니요".Localized(), header);
    }

    public void OpenGatherButtonPowerDownPopup()
    {
        throw new NotImplementedException();
    }

    public void OpenYesImagePopup(string titleText, Sprite sprite, string msg, string btn1Text, Action onBtn1)
    {
        OpenConfirmPopup(msg, onBtn1, null, null, titleText, Header.Normal, btn1Text, "", "", "", "", false, sprite);
    }

    public void OnButton1()
    {
        Sound.Instance.PlayButtonClick();
        
        if (IsOpen) onButton1();
    }

    public void OnButton2()
    {
        Sound.Instance.PlayButtonClick();
        
        if (IsOpen) onButton2();
    }

    public void OnClaimButton()
    {
        Sound.Instance.PlayButtonClick();
        
        if (IsOpen) onClaimButton();
    }

    public void OnCloseButton()
    {
        Sound.Instance.PlayButtonClick();
        
        if (IsOpen) onCloseButton();
    }

    public void OpenSimpleMessage(string inMessage)
    {
        ConDebug.Log(nameof(OpenSimpleMessage));
        Open(inMessage, Close);
    }
    
    public void OpenSimpleMessageWithClickSound(string inMessage)
    {
        ConDebug.Log(nameof(OpenSimpleMessageWithClickSound));
        Sound.Instance.PlayButtonClick();
        Open(inMessage, Close);
    }
    
    public void OpenSimpleMessageWithClickSoundAndResumeDirector(string inMessage)
    {
        ConDebug.Log(nameof(OpenSimpleMessageWithClickSoundAndResumeDirector));
        Sound.Instance.PlayButtonClick();
        Open(inMessage, () =>
        {
            Close();
            IntroDirector.Instance.ResumeDirector();
        });
    }
}