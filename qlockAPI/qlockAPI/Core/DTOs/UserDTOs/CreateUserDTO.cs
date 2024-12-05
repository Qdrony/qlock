namespace qlockAPI.Core.DTOs.UserDTOs
{
    public class CreateUserDTO
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public DateTime Created_At { get; set; } = DateTime.Now;
    }
}
