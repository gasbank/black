using UnityEngine;

public class PlatformInterface : MonoBehaviour {
    static public PlatformInterface instance;
    
    public IPlatformSaveLoadManager saveLoadManager;
    public IPlatformLogManager logManager;
    public IPlatformLogEntryType logEntryType;
    public IPlatformTextHelper textHelper;
    public IPlatformConfirmPopup confirmPopup;
    public IPlatformProgressMessage progressMessage;
    public IPlatformSaveUtil saveUtil;
    public IPlatformNotification notification;
    public IPlatformText text;
    public IPlatformAds ads;
    public IPlatformShortMessage shortMessage;
    public IPlatformBackgroundTimeCompensator backgroundTimeCompensator;
    public IPlatformConfig config;
    public IPlatformLogger logger;
}