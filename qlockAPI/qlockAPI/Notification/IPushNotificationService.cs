namespace qlockAPI.Notification
{
    public interface IPushNotificationService
    {
        Task<bool> SendNotificationAsync(int userId, string title, string body);
    }
}
