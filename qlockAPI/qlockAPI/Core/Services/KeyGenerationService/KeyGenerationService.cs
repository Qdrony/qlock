using System.Security.Cryptography;

namespace qlockAPI.Core.Services.KeyGenerationService;
public class KeyGenerationService : IKeyGenerationService
{
    public string GenerateSecretKey()
    {
        const int keyLength = 32; //256 bits, HS256

        using (var rng = RandomNumberGenerator.Create())
        {
            var secretKeyBytes = new byte[keyLength];
            rng.GetBytes(secretKeyBytes);

            return Convert.ToBase64String(secretKeyBytes);
        }
    }
}
