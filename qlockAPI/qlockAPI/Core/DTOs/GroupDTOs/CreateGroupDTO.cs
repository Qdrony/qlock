namespace qlockAPI.Core.DTOs.GroupDTOs
{
    public class CreateGroupDTO
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int LockId { get; set; }
    }
}
