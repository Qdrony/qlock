using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qlockAPI.Core.Database;
using qlockAPI.Core.DTOs.GroupDTOs;
using qlockAPI.Core.Entities;

namespace qlockAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly QlockContext _context;
        private readonly IMapper _mapper;

        public GroupController(QlockContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //Create group
        [HttpPost]
        [Route("{lockId}/assign-user-to-group")]
        public async Task<IActionResult> AssignUserToGroup(int lockId, [FromBody] AssignUserToGroupDTO assignDto)
        {
            // Ellenőrzés: létezik-e a zár
            var lockEntity = await _context.Locks.FindAsync(lockId);
            if (lockEntity == null)
            {
                return NotFound(new { error = "Lock not found" });
            }

            // Ellenőrzés: létezik-e a felhasználó
            var user = await _context.Users.FindAsync(assignDto.UserId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            // Ellenőrzés: létezik-e a csoport az adott névvel
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Name == assignDto.GroupName);
            if (group == null)
            {
                // Ha a csoport nem létezik, hozzuk létre
                group = new Group
                {
                    Name = assignDto.GroupName,
                    Description = assignDto.Description,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Groups.Add(group);
                await _context.SaveChangesAsync();
            }

            // Ellenőrzés: létezik-e már a hozzárendelés
            var assignGroup = await _context.AssignGroups
                .FirstOrDefaultAsync(ag => ag.UserId == assignDto.UserId && ag.LockId == lockId && ag.GroupId == group.Id);
            if (assignGroup != null)
            {
                return BadRequest(new { error = "User is already assigned to this group for the lock" });
            }

            // Új hozzárendelés létrehozása
            assignGroup = new AssignGroup
            {
                UserId = assignDto.UserId,
                LockId = lockId,
                GroupId = group.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.AssignGroups.Add(assignGroup);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User assigned to group successfully" });
        }


        [HttpDelete("{lockId}/remove-user-from-group")]
        public async Task<IActionResult> RemoveUserFromGroup(int lockId, [FromBody] RemoveUserFromGroupDTO removeDto)
        {
            // Ellenőrzés: létezik-e a zár
            var lockEntity = await _context.Locks.FindAsync(lockId);
            if (lockEntity == null)
            {
                return NotFound(new { error = "Lock not found" });
            }

            // Ellenőrzés: létezik-e a felhasználó
            var user = await _context.Users.FindAsync(removeDto.UserId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            // Ellenőrzés: létezik-e a csoport
            var group = await _context.Groups.FindAsync(removeDto.GroupId);
            if (group == null)
            {
                return NotFound(new { error = "Group not found" });
            }

            // Ellenőrzés: a felhasználó része-e a csoportnak az adott zárnál
            var assignGroup = await _context.AssignGroups
                .FirstOrDefaultAsync(ag => ag.LockId == lockId && ag.UserId == removeDto.UserId && ag.GroupId == removeDto.GroupId);

            if (assignGroup == null)
            {
                return BadRequest(new { error = "User is not part of the group for this lock" });
            }

            // Felhasználó eltávolítása a csoportból
            _context.AssignGroups.Remove(assignGroup);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User removed from group successfully" });
        }


        ////
        //[HttpPost]
        //[Route("/create")]
        //public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDTO dto)
        //{
        //    var lockExists = await _context.Locks.AnyAsync(l => l.Id == dto.LockId);
        //    if (!lockExists)
        //        return NotFound(new { error = "Lock not found" });

        //    var group = new Group
        //    {
        //        Name = dto.Name,
        //        Description = dto.Description,
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    await _context.Groups.AddAsync(group);
        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "Group created", groupId = group.Id });
        //}

        [HttpGet]
        [Route("{lockId}")]
        public async Task<IActionResult> GetGroupsByLock([FromRoute]int lockId)
        {
            var groups = await _context.AssignGroups
                                       .Where(ag => ag.LockId == lockId)
                                       .Include(ag => ag.Group)
                                       .Select(ag => ag.Group)
                                       .Distinct()
                                       .ToListAsync();

            if (!groups.Any())
            {
                return NotFound(new { error = "No groups found for this lock" });
            }

            return Ok(groups.Select(g => new GroupDTO
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                CreatedAt = g.CreatedAt
            }));
        }

        //Get useres with groups
        [HttpGet]
        [Route("{lockId}/users-with-groups")]
        public async Task<IActionResult> GetUsersWithGroupsByLock([FromRoute]int lockId)
        {
            var lockExists = await _context.Locks.AnyAsync(l => l.Id == lockId);
            if (!lockExists)
            {
                return NotFound(new { error = "Lock not found" });
            }

             var usersWithGroups = await _context.Assigns
            .Where(a => a.LockId == lockId)
            .Select(assign => new
            {
            UserId = assign.UserId,
            Groups = _context.AssignGroups
                .Where(ag => ag.LockId == lockId && ag.UserId == assign.UserId)
                .Select(ag => new
                {
                    ag.GroupId,
                    ag.Group.Name
                })
                .ToList()
            })
            .ToListAsync();

            if (!usersWithGroups.Any())
            {
                return NotFound(new { error = "No users or groups found for this lock" });
            }

            return Ok(usersWithGroups);
        }


    }
}
