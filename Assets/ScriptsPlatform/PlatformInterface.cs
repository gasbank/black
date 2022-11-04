using UnityEngine;

public class PlatformInterface : MonoBehaviour
{
    public static PlatformInterface Instance;
    public IPlatformAds ads;
    public IPlatformConfig config;
    public IPlatformConfirmPopup confirmPopup;
    public IPlatformLogEntryType logEntryType;
    public IPlatformLogger logger;
    public IPlatformLogManager logManager;
    public IPlatformNotification notification;
    public IPlatformProgressMessage progressMessage;

    public IPlatformSaveLoadManager saveLoadManager;
    public IPlatformSaveUtil saveUtil;
    public IPlatformShortMessage shortMessage;
    public IPlatformText text;
    public IPlatformTextHelper textHelper;
}