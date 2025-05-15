using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qlockAPI.Core.Database;
using qlockAPI.Core.DTOs.KeyDTOs;
using qlockAPI.Core.DTOs.LockDTOs;
using qlockAPI.Core.Entities;
using qlockAPI.Core.Services.UserService;

namespace qlockAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LockController : ControllerBase
    {
        private readonly QlockContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public LockController(QlockContext context, IMapper mapper, IUserService userService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
        }

        //count how many locks belong to a user
        [HttpGet]
        [Route("count/{userId}")]
        public async Task<IActionResult> CountLocks([FromRoute] int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null)
            {
                return NotFound(new { error = "User not found" });
            }

            var lockCount = await _context.Locks.CountAsync(l => l.Owner == userId);

            return Ok(new { count = lockCount });
        }

        //Get keys for lock
        [HttpGet]
        [Route("{lockId}/keys")]
        public async Task<IActionResult> GetKeysForLock([FromRoute] int lockId)
        {
            var lockEntity = await _context.Locks.AsNoTracking().FirstOrDefaultAsync(l => l.Id == lockId);

            if (lockEntity is null)
            {
                return NotFound(new { error = "Lock not found" });
            }

            var keys = await _context.Keys.AsNoTracking()
                                     .Include(k => k.Lock)
                                     .Include(k => k.User)
                                     .Where(k => k.LockId == lockId)
                                     .ToListAsync();

            if (!keys.Any())
            {
                return NotFound(new { error = "No keys found for this lock" });
            }
            var keysDTO = _mapper.Map<IEnumerable<KeyDTO>>(keys);
            
            return Ok(keysDTO);
        }

        //Create lock
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateLock([FromBody] CreateLockDTO createLockDTO)
        {
            var user = await _context.Users.FindAsync(createLockDTO.Owner);
            if (user is null)
            {
                return NotFound(new { error = "Owner not found" });
            }

            var lockEntity = _mapper.Map<Lock>(createLockDTO);
            lockEntity.Status = "active";

            try
            {
                await _context.Locks.AddAsync(lockEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while creating the lock" });
            }

            return Ok("Lock created");
        }

        //Get lock by id
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<LockDTO>> GetLockById([FromRoute] int id)
        {
            var lockEntity = await _context.Locks.FindAsync(id);

            if (lockEntity is null)
            {
                return NotFound(new { error = "Lock not found" });
            }

            return Ok(_mapper.Map<LockDTO>(lockEntity));
        }

        //Lock update
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateLock([FromRoute] int id, [FromBody] UpdateLockDTO updateLockDTO)
        {
            var lockEntity = await _context.Locks.FindAsync(id);
            if (lockEntity is null)
            {
                return NotFound(new { error = "Lock not found" });
            }

            _mapper.Map(updateLockDTO,lockEntity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while updating the lock" });
            }

            return Ok("Lock updated");
        }

        //Delete lock
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteLock([FromRoute] int id)
        {
            var lockEntity = await _context.Locks.FindAsync(id);
            if (lockEntity is null)
            {
                return NotFound(new { error = "Lock not found" });
            }

            try
            {
                _context.Locks.Remove(lockEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while delete the lock" });
            }

            return Ok("Lock deleted");
        }

    }
}