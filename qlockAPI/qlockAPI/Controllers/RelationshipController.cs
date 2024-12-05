using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qlockAPI.Core.Database;
using qlockAPI.Core.DTOs.UserDTOs;
using qlockAPI.Core.Entities;

namespace qlockAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelationshipController : ControllerBase
    {
        private readonly QlockContext _context;
        private readonly IMapper _mapper;

        public RelationshipController(QlockContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //Get user friends TODO db lekérdezés optimalizáció
        [HttpGet]
        [Route("{userId}/friends")]
        public async Task<ActionResult<IEnumerable<FriendDTO>>> GetFriends([FromRoute]int userId)
        {
            var friends = await _context.Relationships
                .Where(r => (r.UserId == userId || r.FriendId == userId) && r.Type == "friend")
                .Select(r => new
                {
                    FriendId = r.UserId == userId ? r.FriendId : r.UserId
                })
                .ToListAsync();

            if (!friends.Any())
            {
                return NotFound(new { error = "No friends found." });
            }

            var friendDetails = await _context.Users
                                      .Where(u => friends.Select(f => f.FriendId).Contains(u.Id))
                                      .ToListAsync();

            var friendsDTO = _mapper.Map<IEnumerable<FriendDTO>>(friendDetails);

            return Ok(friendsDTO);
        }


        //Add friend
        [HttpPost]
        [Route("{userId}/addfriend/{friendId}")]
        public async Task<IActionResult> AddFriend([FromRoute] int userId, [FromRoute] int friendId)
        {
            if (userId == friendId)
            {
                return BadRequest(new { error = "A user cannot add themselves as a friend." });
            }

            var existingRelationship = await _context.Relationships
            .FirstOrDefaultAsync(r => (r.UserId == userId && r.FriendId == friendId)
                                   || (r.UserId == friendId && r.FriendId == userId));

            if (existingRelationship != null)
            {
                switch (existingRelationship.Type)
                {
                    case "friend":
                        return Conflict(new { error = "You are already friends." });

                    case "pending":
                        if (existingRelationship.UserId == userId)
                        {
                            return Conflict(new { error = "Friend request already sent." });
                        }

                        if (existingRelationship.FriendId == userId)
                        {
                            existingRelationship.Type = "friend";
                            await _context.SaveChangesAsync();
                            return Ok(new { message = "Friend request accepted." });
                        }
                        break;

                    default:
                        return Conflict(new { error = "Unexpected relationship type." });
                }
            }

            var relationship = new Relationship
            {
                UserId = userId,
                FriendId = friendId,
                Type = "pending",
                CreatedAt = DateTime.Now
            };

            try
            {
                _context.Relationships.Add(relationship);
                await _context.SaveChangesAsync();
                return Ok("Friend request sent.");
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An unexpected error occurred." });
            }
        }

        [HttpDelete]
        [Route("{userId}/removefriend/{friendId}")]
        public async Task<IActionResult> RemoveFriend([FromRoute]int userId, [FromRoute]int friendId)
        {
            var relationship = await _context.Relationships
                .FirstOrDefaultAsync(r => (r.UserId == userId && r.FriendId == friendId)
                                       || (r.UserId == friendId && r.FriendId == userId));

            if (relationship is null)
                return NotFound(new { error = "No relationship found." });

            _context.Relationships.Remove(relationship);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Relationship removed." });
        }
    }
}
