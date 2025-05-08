
using qlockAPI.Core.Database;
using qlockAPI.Core.Entities;

namespace qlockAPI.Core.Services.LogService
{
    public class LogService : ILogService
    {
        private readonly QlockContext _context;

        public LogService(QlockContext context)
        {
            _context = context;
        }

        public async Task LogActionAsync(int? userId, int? lockId, int? keyId,DateTime time, string action, string status, string? text = null)
        {
            var log = new Log
            {
                UserId = userId,
                LockId = lockId,
                KeyId = keyId,
                Time = time,
                Action = action,
                Status = status,
                Text = text
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
