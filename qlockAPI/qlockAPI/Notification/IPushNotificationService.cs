using qlockAPI.Core.DTOs.NotificationDTOs;

namespace qlockAPI.Notification
{
    public interface IPushNotificationService
    {
        Task<bool> SendPushNotificationAsync(PushRequest pushRequest);
    }
}
