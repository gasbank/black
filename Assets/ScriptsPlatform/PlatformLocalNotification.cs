using UnityEngine;

public class PlatformLocalNotification : MonoBehaviour {
    [SerializeField]
    PlatformInterface platformInterface;
    
    public void RegisterAllRepeatingNotifications() {
        PlatformNotificationRequest request = PlatformInterface.instance.notification.GetNotificationRequest();
        if (request != null) {
            Platform.instance.RegisterAllNotifications(request.title, request.body, request.largeIcon, request.localHours);
        }
        PlatformInterface.instance.logger.Log("RegisterAllRepeatingNotifications");
    }

    public void RemoveAllRepeatingNotifications() {
        Platform.instance.ClearAllNotifications();
        PlatformInterface.instance.logger.Log("RemoveAllRepeatingNotifications");
    }
}
