namespace qlockAPI.Core.Services.LockService
{
    public interface ILockService
    {
        Task<bool> BlockLockAsync(int lockId);
    }
}
