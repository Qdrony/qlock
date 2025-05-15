using Microsoft.EntityFrameworkCore;
using qlockAPI.Core.Database;
using qlockAPI.Notification;

namespace qlockAPI.Core.Services.KeyService
{
    public class KeyService : IKeyService
    {
        private readonly QlockContext _context;

        public KeyService(QlockContext context)
        {
            _context = context;
        }
        public async Task<bool> ValidateKeyAsync(int keyId)
        {
            var keyEntity = await _context.Keys.FindAsync(keyId);

            if (keyEntity is null)
            {
                throw new ArgumentException("The specified key does not exist.");
            }

            if (keyEntity.RemainingUses == 0)
            {
                keyEntity.Status = "inactive";
            }
            

            if (keyEntity.ExpirationDate is not null && keyEntity.ExpirationDate <= DateTime.Now)
            {
                keyEntity.Status = "inactive";
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the key.", ex);
            }
        }

        public async Task<bool> DecreaseKeyUsageAsync(int keyId)
        {
            var keyEntity = await _context.Keys.FindAsync(keyId);

            if (keyEntity is null)
            {
                throw new ArgumentException("The specified key does not exist.");
            }

            if (keyEntity.RemainingUses > 0)
            {
                keyEntity.RemainingUses--;
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the key.", ex);
            }
        }


        public async Task<bool> IsKeyAssignedToLockAsync(int keyId, int lockId)
        {
            var keyEntity = await _context.Keys.FindAsync(keyId);
            return await _context.Keys.AnyAsync(k => k.Id == keyId && k.LockId == lockId) && keyEntity!.Name == "active";
        }

        public async Task<bool> IsKeyAssignedToUserAsync(int keyId, int userId)
        {
            return await _context.Keys.AnyAsync(k => k.Id == keyId && k.UserId == userId);
        }

        public async Task<bool> IsKeyValidAsync(int keyId)
        {
            var keyEntity = await _context.Keys.FindAsync(keyId);

            if (keyEntity is null || keyEntity.Status == "inactive")
            {
               return false;
            }

            var now = TimeOnly.FromDateTime(DateTime.Now);

            var isNotExpired = keyEntity.ExpirationDate is null || keyEntity.ExpirationDate >= DateTime.Now;
            var hasUsesRemaining = keyEntity.RemainingUses == -1 || keyEntity.RemainingUses > 0;
            bool isWithinTime;

            if (keyEntity.StartTime is null || keyEntity.EndTime is null)
            {
                isWithinTime = true;
            }
            else if (keyEntity.StartTime <= keyEntity.EndTime)
            {
                isWithinTime = now >= keyEntity.StartTime && now <= keyEntity.EndTime;
            }
            else
            {
                isWithinTime = now >= keyEntity.StartTime || now <= keyEntity.EndTime;
            }


            return isNotExpired && hasUsesRemaining && isWithinTime;
        }

    }
}
