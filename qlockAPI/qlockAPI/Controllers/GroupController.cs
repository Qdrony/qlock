using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qlockAPI.Core.Database;
using qlockAPI.Core.DTOs.GroupDTOs;
using qlockAPI.Core.DTOs.UserDTOs;
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

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateGroup([FromBody]CreateGroupDTO group)
        {
            _context.Groups.Add(_mapper.Map<CreateGroupDTO, Group>(group));
            await _context.SaveChangesAsync();
            return Ok(new { message = "Group created successfully" });
        }

        [HttpDelete]
        [Route("delete/{groupId}")]
        public async Task<IActionResult> DeleteGroup([FromRoute]int groupId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Route("{groupId}/{userId}")]
        public async Task<IActionResult> AssignUserToGroup([FromRoute]int groupId, [FromRoute]int userId)
        {
            var assign = await _context.Assigns.FirstOrDefaultAsync(a => a.GroupId == groupId && a.UserId == userId);
            if (assign != null)
            {
                return BadRequest(new { error = "User is already assigned to this group" });
            }

            var result = new Assign { GroupId = groupId, UserId = userId, AssignedAt = DateTime.UtcNow };
            _context.Assigns.Add(result);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete]
        [Route("{groupId}/{userId}")]
        public async Task<IActionResult> RemoveUserFromGroup([FromRoute]int groupId, [FromRoute]int userId)
        {
            var assign = await _context.Assigns.FirstOrDefaultAsync(a => a.GroupId == groupId && a.UserId == userId);
            if (assign == null)
            {
                return NotFound();
            }
            _context.Assigns.Remove(assign);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        [Route("locks/{lockId}")]
        public async Task<IActionResult> GetGroupsFromLockWithUsers([FromRoute]int lockId)
        {
            var groups = await _context.Groups
                .Where(g => g.LockId == lockId)
                .ToListAsync();

            var result = new List<object>();

            foreach (var g in groups)
            {
                var users = await _context.Assigns
                    .Where(a => a.GroupId == g.Id)
                    .Include(a => a.User)
                    .Select(a => new
                    {
                        Id = a.User.Id,
                        Name = a.User.Name
                    })
                    .ToListAsync();

                result.Add(new
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    Users = users
                });
            }

            return Ok(result);
        }

        [HttpGet]
        [Route("lock/{lockId}/groups")]
        public async Task<IActionResult> GetGroupsFromLock([FromRoute] int lockId)
        {
            var groups = await _context.Groups
                .Where(g => g.LockId == lockId)
                .ToListAsync();

            return Ok(groups);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroupById([FromRoute]int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }
            return Ok(group);
        }

        [HttpGet]
        [Route("{groupId}/users")]
        public async Task<IActionResult> GetUsersFromGroup([FromRoute]int groupId)
        {
            var users = await _context.Assigns
                .Where(a => a.GroupId == groupId)
                .Include(a => a.User)
                .Select(a => new UserViewDTO { 
                    Id = a.UserId,
                    Email = a.User.Email,
                    Name = a.User.Name
                })
                .ToListAsync();

            if (users is null)
            {
                return NotFound(new { message = "No users found in this group"});
            }

            return Ok(users);
        }






        ////Create group
        //[HttpPost]
        //[Route("add/{lockId}/{groupId}/{userId}")]
        //public async Task<IActionResult> AssignUserToGroup([FromRoute] int lockId, [FromRoute] int groupId, [FromRoute] int userId)
        //{
        //    // Ellenőrzés: létezik-e a zár
        //    var lockEntity = await _context.Locks.FindAsync(lockId);
        //    if (lockEntity == null)
        //    {
        //        return NotFound(new { error = "Lock not found" });
        //    }

        //    // Ellenőrzés: létezik-e a felhasználó
        //    var user = await _context.Users.FindAsync(userId);
        //    if (user == null)
        //    {
        //        return NotFound(new { error = "User not found" });
        //    }

        //    // Ellenőrzés: létezik-e a csoport
        //    var group = await _context.Groups.FindAsync(groupId);
        //    if (group == null)
        //    {
        //        return NotFound(new { error = "Group not found" });
        //    }

        //    // Ellenőrzés: létezik-e már a assign hozzárendelés
        //    var assign = await _context.Assigns
        //        .FirstOrDefaultAsync(a => a.UserId == userId && a.LockId == lockId);
        //    // Ha nem létezik, akkor létrehozás
        //    if (assign is null)
        //    {
        //        assign = new Assign
        //        {
        //            UserId = userId,
        //            LockId = lockId,
        //            AssignedAt = DateTime.UtcNow
        //        };

        //        _context.Assigns.Add(assign);
        //        await _context.SaveChangesAsync();
        //    }

        //    // Ellenőrzés: létezik-e már a hozzárendelés
        //    var assignGroup = await _context.AssignGroups
        //        .FirstOrDefaultAsync(ag => ag.UserId == userId && ag.LockId == lockId && ag.GroupId == group.Id);
        //    if (assignGroup != null)
        //    {
        //        return BadRequest(new { error = "User is already assigned to this group for the lock" });
        //    }

        //    // Új hozzárendelés létrehozása
        //    assignGroup = new AssignGroup
        //    {
        //        UserId = userId,
        //        LockId = lockId,
        //        GroupId = group.Id,
        //        CreatedAt = DateTime.UtcNow
        //    };

        //    _context.AssignGroups.Add(assignGroup);
        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "User assigned to group successfully" });
        //}


        //[HttpDelete("delete/{lockId}/{groupId}/{userId}")]
        //public async Task<IActionResult> RemoveUserFromGroup([FromRoute] int lockId, [FromRoute] int userId, [FromRoute] int groupId)
        //{
        //    // Ellenőrzés: létezik-e a zár
        //    var lockEntity = await _context.Locks.FindAsync(lockId);
        //    if (lockEntity == null)
        //    {
        //        return NotFound(new { error = "Lock not found" });
        //    }

        //    // Ellenőrzés: létezik-e a felhasználó
        //    var user = await _context.Users.FindAsync(userId);
        //    if (user == null)
        //    {
        //        return NotFound(new { error = "User not found" });
        //    }

        //    // Ellenőrzés: létezik-e a csoport
        //    var group = await _context.Groups.FindAsync(groupId);
        //    if (group == null)
        //    {
        //        return NotFound(new { error = "Group not found" });
        //    }

        //    // Ellenőrzés: a felhasználó része-e a csoportnak az adott zárnál
        //    var assignGroup = await _context.AssignGroups
        //        .FirstOrDefaultAsync(ag => ag.LockId == lockId && ag.UserId == userId && ag.GroupId == groupId);

        //    if (assignGroup == null)
        //    {
        //        return BadRequest(new { error = "User is not part of the group for this lock" });
        //    }

        //    // Felhasználó eltávolítása a csoportból
        //    _context.AssignGroups.Remove(assignGroup);
        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "User removed from group successfully" });
        //}

        //////Get groups
        ////[HttpGet]
        ////[Route("{lockId}")]
        ////public async Task<IActionResult> GetGroupsByLock([FromRoute]int lockId)
        ////{
        ////    var groups = await _context.AssignGroups
        ////                               .Where(ag => ag.LockId == lockId)
        ////                               .Include(ag => ag.Group)
        ////                               .Select(ag => ag.Group)
        ////                               .Distinct()
        ////                               .ToListAsync();

        ////    if (!groups.Any())
        ////    {
        ////        return NotFound(new { error = "No groups found for this lock" });
        ////    }

        ////    return Ok(groups.Select(g => new GroupDTO
        ////    {
        ////        Id = g.Id,
        ////        Name = g.Name,
        ////        Description = g.Description,
        ////        CreatedAt = g.CreatedAt
        ////    }));
        ////}

        //[HttpGet]
        //[Route("{lockId}")]
        //public async Task<IActionResult> GetGroupsByLock([FromRoute] int lockId)
        //{
        //    var groups = await _context.AssignGroups
        //        .Where(ag => ag.LockId == lockId)
        //        .Include(ag => ag.Group)
        //        .Include(ag => ag.Assign)
        //        .ThenInclude(a => a.User)
        //        .ToListAsync();

        //    if (!groups.Any())
        //    {
        //        return NotFound(new { error = "No groups found for this lock" });
        //    }

        //    var result = groups.GroupBy(ag => ag.Group)
        //        .Select(g => new GroupWithUsersDTO
        //        {
        //            Id = g.Key.Id,
        //            Name = g.Key.Name,
        //            Description = g.Key.Description,
        //            Users = g.Select(ag => ag.Assign.User)
        //                     .Where(u => u != null)
        //                     .Distinct()
        //                     .Select(u => new UserViewDTO
        //                     {
        //                         Id = u.Id,
        //                         Name = u.Name,
        //                         Email = u.Email
        //                     }).ToList()
        //        })
        //        .ToList();

        //    return Ok(result);
        //}

        ////Get users with groups
        //[HttpGet]
        //[Route("{lockId}/users-with-groups")]
        //public async Task<IActionResult> GetUsersWithGroupsByLock([FromRoute] int lockId)
        //{
        //    var lockExists = await _context.Locks.AnyAsync(l => l.Id == lockId);
        //    if (!lockExists)
        //    {
        //        return NotFound(new { error = "Lock not found" });
        //    }

        //    var usersWithGroups = await _context.Assigns
        //   .Where(a => a.LockId == lockId)
        //   .Select(assign => new
        //   {
        //       UserId = assign.UserId,
        //       Groups = _context.AssignGroups
        //       .Where(ag => ag.LockId == lockId && ag.UserId == assign.UserId)
        //       .Select(ag => new
        //       {
        //           ag.GroupId,
        //           ag.Group.Name
        //       })
        //       .ToList()
        //   })
        //   .ToListAsync();

        //    if (!usersWithGroups.Any())
        //    {
        //        return NotFound(new { error = "No users or groups found for this lock" });
        //    }

        //    return Ok(usersWithGroups);
        //}

        //[HttpDelete]
        //[Route("delete/{lockId}/{groupId}")]
        //public async Task<IActionResult> DeleteGroupFromLock([FromRoute] int lockId, [FromRoute] int groupId)
        //{
        //    var assignGroups = await _context.AssignGroups
        //        .Where(ag => ag.LockId == lockId && ag.GroupId == groupId)
        //        .ToListAsync();

        //    if (!assignGroups.Any())
        //    {
        //        return NotFound(new { error = "The specified group is not assigned to this lock" });
        //    }

        //    _context.AssignGroups.RemoveRange(assignGroups);
        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "Group successfully removed from lock" });
        //}

    }
}