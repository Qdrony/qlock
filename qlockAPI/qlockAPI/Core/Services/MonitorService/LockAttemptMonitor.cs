using Microsoft.Extensions.Caching.Memory;

namespace qlockAPI.Core.Services.MonitorService
{
    public class LockAttemptMonitor : ILockAttemptMonitor
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<LockAttemptMonitor> _logger;

        public LockAttemptMonitor(IMemoryCache cache, ILogger<LockAttemptMonitor> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public Task<bool> RegisterFailedAttemptAsync(int lockId)
        {
            string key = $"lock-attempts-{lockId}";
            List<DateTime> attempts = _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return new List<DateTime>();
            });

            attempts.Add(DateTime.UtcNow);

            // Csak az utolsó 10 perc próbálkozásait számoljuk
            attempts = attempts.Where(a => a > DateTime.UtcNow.AddMinutes(-10)).ToList();

            _cache.Set(key, attempts, TimeSpan.FromMinutes(10));

            // Ha 5 vagy több próbálkozás volt, true-t ad vissza
            return Task.FromResult(attempts.Count >= 5);
        }
    }

}
