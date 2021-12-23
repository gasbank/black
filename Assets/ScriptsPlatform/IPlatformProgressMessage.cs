public interface IPlatformProgressMessage
{
    string MessageText { get; set; }
    bool IsOpen { get; }
    void Close();
    void Open(string text);
    void ForceBackButtonActive();
    void CloseButtonPopup();
    void DisableCloseButton();
    void PushCloseButton();
}