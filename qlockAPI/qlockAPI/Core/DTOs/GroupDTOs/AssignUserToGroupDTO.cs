namespace qlockAPI.Core.DTOs.GroupDTOs
{
    public class AssignUserToGroupDTO
    {
        public int UserId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
