namespace qlockAPI.Core.DTOs.ActionDTOs
{
    public class LockOpenRequestDTO
    {
        public int KeyId { get; set; }
        public int LockId { get; set; }
        public int UserId { get; set; }
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;
    }
}
