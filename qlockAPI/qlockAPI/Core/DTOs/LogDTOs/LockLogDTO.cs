using qlockAPI.Core.Entities;

namespace qlockAPI.Core.DTOs.LogDTOs
{
    public class LockLogDTO
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public string Status { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public int KeyId { get; set; }
        public int LockId { get; set; }
        public string KeyName { get; set; }
        public string LockName { get; set; }
    }
}
