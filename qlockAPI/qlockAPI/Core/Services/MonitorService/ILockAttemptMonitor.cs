namespace qlockAPI.Core.Services.MonitorService
{
    public interface ILockAttemptMonitor
    {
        Task<bool> RegisterFailedAttemptAsync(int lockId);
    }
}
