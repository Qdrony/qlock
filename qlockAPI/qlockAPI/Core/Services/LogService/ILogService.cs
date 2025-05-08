namespace qlockAPI.Core.Services.LogService
{
    public interface ILogService
    {
        Task LogActionAsync(int? userId, int? lockId, int? keyId,DateTime time, string action, string status, string? text = null);
    }
}
