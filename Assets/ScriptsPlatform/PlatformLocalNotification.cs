using UnityEngine;

public class PlatformLocalNotification : MonoBehaviour
{
    public static void RegisterAllRepeatingNotifications()
    {
        var request = PlatformInterface.instance.notification.GetNotificationRequest();
        if (request != null)
        {
            Platform.instance.RegisterAllNotifications(request.title, request.body, request.largeIcon,
                request.localHours);
        }

        PlatformInterface.instance.logger.Log(nameof(RegisterAllRepeatingNotifications));
    }

    public static void RemoveAllRepeatingNotifications()
    {
        Platform.instance.ClearAllNotifications();
        PlatformInterface.instance.logger.Log(nameof(RemoveAllRepeatingNotifications));
    }
}