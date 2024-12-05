namespace qlockAPI.Core.Services.KeyService
{
    public interface IKeyService
    {
        Task<bool> IsKeyValidAsync(int keyId);
        Task<bool> IsKeyAssignedToLockAsync(int keyId, int lockId);
        Task<bool> IsKeyAssignedToUserAsync(int keyId, int userId);
        Task<bool> DecreaseKeyUsageAsync(int keyId);
        Task<bool> ValidateKeyAsync(int keyId);
    }
}
