using qlockAPI.Core.Entities;

namespace qlockAPI.Core.Services.UserService
{
    public interface IUserService
    {
        Task<User?> GetUserByEmailAsync(string email);
        bool VerifyPassword(string plainPassword, string hashedPassword);
        string GenerateJwtToken(User user);
        string HashPassword(string password);
    }
}
