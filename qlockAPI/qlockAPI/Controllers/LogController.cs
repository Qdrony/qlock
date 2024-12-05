using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qlockAPI.Core.Database;
using qlockAPI.Core.DTOs.LogDTOs;

namespace qlockAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly QlockContext _context;
        private readonly IMapper _mapper;

        public LogController(QlockContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Get logs for user
        [HttpGet]
        [Route("user/{userId}/logs")]
        public async Task<IActionResult> GetLogsByUser([FromRoute]int userId)
        {
            var logs = await _context.Logs
                .Where(log => log.UserId == userId)
                .ToListAsync();

            if (logs == null || logs.Count == 0)
            {
                return NotFound(new { error = "No logs found for this user." });
            }

            return Ok(logs);
        }

        // Get logs for a specific lock
        [HttpGet]
        [Route("lock/{lockId}/logs")]
        public async Task<ActionResult<IEnumerable<LockLogDTO>>> GetLogsByLock([FromRoute]int lockId)
        {
            var logs = await _context.Logs.AsNoTracking()
                .Where(log => log.LockId == lockId)
                .Include(u => u.User)       
                .Include(k => k.Key)        
                .Include(l => l.Lock)       
                .ToListAsync();

            if (logs == null || logs.Count == 0)
            {
                return NotFound(new { error = "No logs found for this lock." });
            }

            var logsDTO = _mapper.Map<IEnumerable<LockLogDTO>>(logs);

            return Ok(logsDTO);
        }

        // Get logs for a specific key
        [HttpGet("key/{keyId}/logs")]
        public async Task<IActionResult> GetLogsByKey(int keyId)
        {
            var logs = await _context.Logs
                .Where(log => log.KeyId == keyId)
                .ToListAsync();

            if (logs == null || logs.Count == 0)
            {
                return NotFound(new { error = "No logs found for this key." });
            }

            return Ok(logs);
        }

    }
}
