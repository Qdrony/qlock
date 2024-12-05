namespace qlockAPI.Core.DTOs.GroupDTOs
{
    public class CreateGroupDTO
    {
        public int LockId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
