public static class PlatformIosNative {
#if UNITY_IOS
    [System.Runtime.InteropServices.DllImport("__Internal")]
    public static extern void saveToCloudPrivate(string playerId, string data, string loginErrorTitle,
        string loginErrorMessage, string confirmMessage);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    public static extern void loadFromCloudPrivate(string playerId, string loginErrorTitle, string loginErrorMessage,
        string confirmMessage);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    public static extern void clearAllNotifications();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    public static extern void sendMail(string title, string body, string recipient, string attachment);
#else
    public static void saveToCloudPrivate(string playerID, string data, string loginErrorTitle,
        string loginErrorMessage, string confirmMessage) {
        UnityEngine.Debug.LogError("PlatformIosNative.saveToCloudPrivate: Unsupported method call");
    }

    public static void loadFromCloudPrivate(string playerID, string loginErrorTitle, string loginErrorMessage,
        string confirmMessage) {
        UnityEngine.Debug.LogError("PlatformIosNative.loadFromCloudPrivate: Unsupported method call");
    }
#endif
}