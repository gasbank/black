public interface IPlatformProgressMessage {
    void Close();
    void Open(string text);
    void ForceBackButtonActive();
    void CloseButtonPopup();
    void DisableCloseButton();
    void PushCloseButton();
    string MessageText { get; set; }
    bool IsOpen { get; }
}