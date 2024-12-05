using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using qlockAPI.Core.Entities;

namespace qlockAPI.Core.DTOs.KeyDTOs
{
    public class CreateKeyDTO
    {
        public string Type { get; set; } = null!;

        public DateTime? ExpirationDate { get; set; }

        public bool? Used { get; set; } = false;

        public int UserId { get; set; }
        public int LockId { get; set; }
    }
}
