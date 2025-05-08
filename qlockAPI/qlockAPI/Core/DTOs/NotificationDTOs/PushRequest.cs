namespace qlockAPI.Core.DTOs.NotificationDTOs
{
    public class PushRequest
    {
        public string Token { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
