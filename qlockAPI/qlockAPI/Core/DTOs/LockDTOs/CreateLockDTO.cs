using qlockAPI.Core.Entities;

namespace qlockAPI.Core.DTOs.LockDTOs
{
    public class CreateLockDTO
    {
        public string Name { get; set; }
        public int Owner { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }
}
