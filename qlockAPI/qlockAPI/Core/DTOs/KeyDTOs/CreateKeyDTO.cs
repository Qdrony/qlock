using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using qlockAPI.Core.Entities;

namespace qlockAPI.Core.DTOs.KeyDTOs
{
    public class CreateKeyDTO
    {
        public DateTime? ExpirationDate { get; set; }
        public int UserId { get; set; }
        public int LockId { get; set; }
        public int RemainingUses { get; set; }
        public string? Name { get; set; }
    }
}
