namespace qlockAPI.Core.DTOs.UserDTOs
{
    public class ChangePasswordDTO
    {
        public int Id { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
    }
}
