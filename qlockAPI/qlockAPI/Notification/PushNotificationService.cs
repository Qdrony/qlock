using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using qlockAPI.Core.DTOs.NotificationDTOs;
using qlockAPI.Notification;

public class PushNotificationService : IPushNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(HttpClient httpClient, ILogger<PushNotificationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }


    public async Task<bool> SendPushNotificationAsync(PushRequest pushRequest)
    {
        var notification = new
        {
            to = pushRequest.Token,
            title = pushRequest.Title,
            body = pushRequest.Body,
            sound = "default",
            channelId = "default"
        };

        var json = JsonSerializer.Serialize(notification);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://exp.host/--/api/v2/push/send", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Hiba történt a push értesítés küldésekor: {Response}", responseContent);
            return false;
        }
        else
        {
            _logger.LogInformation("Sikeres push értesítés: {Response}", responseContent);
            return true;
        }
    }
}