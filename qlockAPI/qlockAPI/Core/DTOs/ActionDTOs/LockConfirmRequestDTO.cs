namespace qlockAPI.Core.DTOs.ActionDTOs
{
    public class LockConfirmRequestDTO
    {
        public int KeyId { get; set; }
        public int LockId { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; }
        public DateTime OpenAt { get; set; }
    }
}
