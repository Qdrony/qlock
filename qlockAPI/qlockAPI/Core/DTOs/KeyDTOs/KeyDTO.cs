namespace qlockAPI.Core.DTOs.KeyDTOs
{
    public class KeyDTO
    {
        public int Id { get; set; }
        public string SecretKey { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int RemainingUses { get; set; }
        public int UserId { get; set; }
        public bool Used { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LockId { get; set; }
        public string LockName { get; set; }
    }
}
