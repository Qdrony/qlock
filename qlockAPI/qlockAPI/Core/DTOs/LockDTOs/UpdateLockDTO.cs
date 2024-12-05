using qlockAPI.Core.Entities;

namespace qlockAPI.Core.DTOs.LockDTOs
{
    public class UpdateLockDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
    }
}
