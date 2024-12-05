namespace qlockAPI.Core.DTOs.LogDTOs
{
    public class LogDTO
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public int? LockId { get; set; }

        public int? KeyId { get; set; }

        public DateTime? Time { get; set; }

        public string Action { get; set; } = null!;

        public string Status { get; set; } = null!;

        public string? Text { get; set; }
    }
}
