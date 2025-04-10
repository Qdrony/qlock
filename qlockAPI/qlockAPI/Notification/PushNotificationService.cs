using Microsoft.EntityFrameworkCore;
using qlockAPI.Core.Database;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace qlockAPI.Notification
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly QlockContext _context;

        public PushNotificationService(QlockContext context)
        {
            _context = context;
        }
        public async Task<bool> SendNotificationAsync(int userId, string title, string message)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.Pushtoken))
                return false;

            using var client = new HttpClient();
            var requestBody = new
            {
                to = user.Pushtoken,
                sound = "default",
                title = title,
                body = message
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://exp.host/--/api/v2/push/send", content);

            return response.IsSuccessStatusCode;
        }

    }
}
