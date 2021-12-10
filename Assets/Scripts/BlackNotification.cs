public class BlackNotification : IPlatformNotification {
    public PlatformNotificationRequest GetNotificationRequest() {
        return BlackPlatform.GetPlatformNotificationRequest();
    }
}