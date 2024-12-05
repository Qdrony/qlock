namespace qlockAPI.Notification
{
    public interface IPushNotificationService
    {
        Task SendNotificationAsync(int userId, string title, string body);
    }
}
