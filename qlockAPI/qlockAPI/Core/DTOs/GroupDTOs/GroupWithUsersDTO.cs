using qlockAPI.Core.DTOs.UserDTOs;

namespace qlockAPI.Core.DTOs.GroupDTOs
{
    public class GroupWithUsersDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public List<UserViewDTO> Users { get; set; }
    }
}
