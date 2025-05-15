using Microsoft.EntityFrameworkCore;
using qlockAPI.Core.Database;

namespace qlockAPI.Core.Services.LockService
{
    public class LockService : ILockService
    {
        private readonly QlockContext _context;

        public LockService(QlockContext context)
        {
            _context = context;
        }
        public async Task<bool> BlockLockAsync(int lockId)
        {
            var lockEntity = await _context.Locks.FindAsync(lockId);
            if (lockEntity == null)
            {
                return false;
            }

            lockEntity.Status = "Blocked";

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
