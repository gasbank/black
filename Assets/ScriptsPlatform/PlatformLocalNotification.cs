using UnityEngine;

public class PlatformLocalNotification : MonoBehaviour
{
    // 알림은 일단 켜지 말자. 뭐 줄 게 없다...
    public static void RegisterAllRepeatingNotifications()
    {
    }

    public static void RemoveAllRepeatingNotifications()
    {
        Platform.Instance.ClearAllNotifications();
        PlatformInterface.Instance.logger.Log(nameof(RemoveAllRepeatingNotifications));
    }
}