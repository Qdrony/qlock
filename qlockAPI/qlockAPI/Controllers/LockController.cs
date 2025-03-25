using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qlockAPI.Core.Database;
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
                                     .Where(k => k.LockId == lockId)
                                     .ToListAsync();

            if (!keys.Any())
            {
                return NotFound(new { error = "No keys found for this lock" });
            }

            return Ok(keys);
        }

        //Add user to lock
        [HttpPost]
        [Route("{userId}/addto/{lockId}")]
        public async Task<IActionResult> AddUserToLock([FromRoute]int userId, [FromRoute]int lockId)
        {
            var user = await _context.Users.FindAsync(userId);
            var lockEntity = await _context.Locks.FindAsync(lockId);

            if (user is null || lockEntity is null)
            {
                return NotFound(new { error = "User or lock not found" });
            }

            if (await _context.Assigns.AnyAsync(a => a.UserId == userId && a.LockId == lockId))
            {
                return Conflict(new { error = "User is already assigned to this lock" });
            }

            var assign = new Assign
            {
                UserId = userId,
                LockId = lockId,
                AssignedAt = DateTime.UtcNow
            };

            await _context.Assigns.AddAsync(assign);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User assigned to lock successfully" });
        }

        //Remove user from lock
        [HttpDelete]
        [Route("remove/{userId}/from/{lockId}")]
        public async Task<IActionResult> RemoveUserFromLock([FromRoute]int userId, [FromRoute]int lockId)
        {
            var assign = await _context.Assigns.FirstOrDefaultAsync(a => a.UserId == userId && a.LockId == lockId);

            if (assign is null)
            {
                return NotFound(new { error = "Assignment not found" });
            }

            _context.Assigns.Remove(assign);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User removed from lock successfully" });
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