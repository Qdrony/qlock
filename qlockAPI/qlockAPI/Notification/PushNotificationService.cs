using System.Net.Http.Headers;

namespace qlockAPI.Notification
{
    public class PushNotificationService : IPushNotificationService
    {
        public async Task SendNotificationAsync(int userId, string title, string body)
        {
            var userDeviceToken = await GetUserDeviceTokenAsync(userId); // Lekérdezi a felhasználó eszközének tokenjét

            if (!string.IsNullOrEmpty(userDeviceToken))
            {
                var payload = new
                {
                    to = userDeviceToken,
                    notification = new
                    {
                        title = title,
                        body = body
                    }
                };

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", "YOUR_SERVER_KEY");

                var response = await httpClient.PostAsJsonAsync("https://fcm.googleapis.com/fcm/send", payload);
                if (!response.IsSuccessStatusCode)
                {
                    // Logolás hibás push értesítés esetén TODO
                }
            }
        }

        private Task<string> GetUserDeviceTokenAsync(int userId)
        {
            //A token lekérdezését az adatbázisból TODO
            throw new NotImplementedException();
        }
    }
}
