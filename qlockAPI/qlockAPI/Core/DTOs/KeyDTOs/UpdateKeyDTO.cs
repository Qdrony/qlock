using System.Reflection.Metadata.Ecma335;

namespace qlockAPI.Core.DTOs.KeyDTOs
{
    public class UpdateKeyDTO
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int RemainingUses { get; set; }
        public string Name { get; set; }
    }
}
