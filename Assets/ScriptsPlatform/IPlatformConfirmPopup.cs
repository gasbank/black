using System;
using UnityEngine;
using UnityEngine.UI;

public interface IPlatformConfirmPopup {
    void OpenYesNoPopup(string msg, Action onYes, Action onNo);
    void Open(string msg);
    void Close();
    void OpenTwoButtonPopup_Update(string text, Action close, Action action);
    void OpenYesNoImagePopup(string textTitle, string msg, Action onYes, Sprite sprite);

    void OpenYesNoImagePopup(string titleText, Sprite sprite, string msg, string btn1Text, string btn2Text,
        Action onBtn1, Action onBtn2);

    void OpenOneButtonPopup(string msg, Action onYes, string titleText, string confirmText,
        Header header = Header.Normal);

    void OpenInputFieldPopup(string msg, Action onYes, Action onNo, string titleText, Header header,
        string inputFieldText, string inputFieldPlaceholder, InputField.CharacterValidation validation = InputField.CharacterValidation.None);

    void Open(string msg, Action onYes, string titleText, Header header = Header.Normal);

    void OpenRiceClaimPopup(string msg, string claimBtnText, Action onClaim, string titleText,
        Header header = Header.Normal, int autoCloseSec = -1);

    void OpenConfirmPopup(string msg, Action onBtn1, Action onBtn2, Action onBtn3,
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
        Action<int> coreIndexSelectionCallback = null);

    void OpenYesImagePopup(string titleText, string msg, Action onYes, Sprite sprite);

    void OpenTwoButtonPopup(string msg, Action onBtn1, Action onBtn2, string titleText,
        string btn1Text, string btn2Text, Header header = Header.Normal,
        ShowPosition showPosition = ShowPosition.Center, Action onCloseBtn = null,
        bool allowTouchEventsOutsidePopup = false);

    void OpenCoreIndexSelectionPopup(string msg, string titleText, string btn1Text, Action<int> buttonCallback);

    void Open(string msg, Action onYes = null);

    void ActivatePopupImage(Sprite popupSprite, float popupImageTopOffset, int popupImageHeight);
    void ActivateTopImage(Sprite sprite);

    void OpenGemClaimPopup(string msg, string claimBtnText, Action onClaim, string titleText,
        Header header = Header.Normal, int autoCloseSec = -1);
    
    GameObject ClaimImageGameObject { get; }
    string InputFieldText { get; }
    string MessageText { get; set; }
    bool IsOpen { get; }
}

public enum Header {
    Normal,
    Decorated,
    Decorated2,
    Decorated3,
}

public enum WidthType {
    Normal = 480,
    Narrow = 380,
}

public enum ShowPosition {
    Center,
    Bottom,
}

public enum CoreIndexSelectionMode {
    DontCare,
    CustomSpawnAtOnce,
    SimpleButton,
}
