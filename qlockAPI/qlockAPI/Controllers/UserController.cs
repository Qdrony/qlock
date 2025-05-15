using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using qlockAPI.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using qlockAPI.Core.DTOs.UserDTOs;
using qlockAPI.Core.Services.UserService;
using qlockAPI.Core.DTOs.KeyDTOs;
using qlockAPI.Core.Database;
using qlockAPI.Core.DTOs.LockDTOs;
using Microsoft.AspNetCore.Cors;
using qlockAPI.Notification;
using qlockAPI.Core.DTOs.NotificationDTOs;

namespace qlockAPI.Controllers
{
    [EnableCors("AllowAllHeaders")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly QlockContext _context;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IPushNotificationService _pushNotificationService;

        public UserController(QlockContext context,IMapper mapper,IUserService userService,IPushNotificationService pushNotificationService)
        {
            _context = context;
            _mapper = mapper;
            _userService = userService;
            _pushNotificationService = pushNotificationService;
        }

        //Change password
        [HttpPut]
        [Route("{id}/change-password")]
        public async Task<IActionResult> ChangePassword([FromRoute]int id, [FromBody] ChangePasswordDTO changePasswordDTO)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
            {
                return NotFound(new { error = "User not found" });
            }

            if (!_userService.VerifyPassword(changePasswordDTO.Password, user.Password))
            {
                return BadRequest(new { error = "Current password is incorrect" });
            }

            user.Password = _userService.HashPassword(changePasswordDTO.NewPassword);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while changing the password" });
            }
        }

        //Login
        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (string.IsNullOrEmpty(loginDTO.Email) || string.IsNullOrEmpty(loginDTO.Password))
            {
                return BadRequest(new { error = "Email and password are required" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDTO.Email);
            if (user is null || !_userService.VerifyPassword(loginDTO.Password, user.Password))
            {
                return Unauthorized(new { error = "Invalid credentials" });
            }

            var token = _userService.GenerateJwtToken(user);
            user.LastLogin = DateTime.Now;

            PushRequest pushRequest = new PushRequest
            {
                Token = user.Pushtoken!,
                Title = "Login",
                Body = "You have successfully logged in."
            };

            if (_pushNotificationService.SendPushNotificationAsync(pushRequest).Result)
            {
                //TODO log success
            }

            await _context.SaveChangesAsync();
            return Ok(new { token, user = _mapper.Map<UserViewDTO>(user) });
        }

        //Delete User
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute]int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
            {
                return NotFound(new { error = "User not found" });
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while deleting the user"});
            }

            return Ok(new { message = "User deleted" });
        }

        //Update user
        [HttpPut]
        [Route("update/{id}")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDTO updateUserDTO,[FromRoute] int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
            {
                return NotFound(new { error = "User not found" });
            }

            if (await _context.Users.AnyAsync(u => u.Email == updateUserDTO.Email && u.Id != id))
            {
                return Conflict(new { error = "Email already used" });
            }

            _mapper.Map(updateUserDTO, user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while updating the user" });
            }

            return Ok(new { message = "User updated"});
        }

        //Read all
        [HttpGet]
        [Route("getall")]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
            var users = await _context.Users.AsNoTracking().ToListAsync();

            if (users is null)
            {
                return NotFound("Users not found!");
            }

            return Ok(_mapper.Map<IEnumerable<UserViewDTO>>(users));
        }

        //Create
        [AllowAnonymous]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateUser([FromBody]CreateUserDTO createUserDTO)
        {

            if (await _context.Users.AnyAsync(u => u.Email == createUserDTO.Email))
            {
                return Conflict(new { error = "Email already registered" });
            }

            var newUser = _mapper.Map<User>(createUserDTO);

            newUser.Password = _userService.HashPassword(createUserDTO.Password);
            newUser.Status = "active";


            await _context.Users.AddAsync(newUser);
            if (0 == await _context.SaveChangesAsync())
            {
                return StatusCode(500, new { error = "an unexpected error occurred" });
            }

               return Ok(new { message = "User created" });
        }

        //User by id
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<UserViewDTO>> GetUserById([FromRoute] int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user is null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(_mapper.Map<UserViewDTO>(user));
        }

        //User get locks
        [HttpGet]
        [Route("{userId}/locks")]
        public async Task<ActionResult<IEnumerable<LockDTO>>> GetUserLocks([FromRoute]int userId)
        {
            var locks = await _context.Locks.AsNoTracking()
                                       .Where(l => l.Owner == userId)
                                       .ToListAsync();

            if (locks is null)
            {
                return NotFound(new { error = "Lock(s) not found" });
            }

            var locksDTO = _mapper.Map<IEnumerable<LockDTO>>(locks);

            return Ok(locksDTO);
        }

        //User get keys
        [HttpGet]
        [Route("{userId}/keys")]
        public async Task<ActionResult<IEnumerable<KeyDTO>>> GetUserKeys([FromRoute] int userId)
        {
            var keys = await _context.Keys.Include(k => k.Lock)
                                     .Include(k => k.User)
                                     .Where(k => k.UserId == userId)
                                     .ToListAsync();

            if (keys is null)
            {
                return NotFound(new { error = "Keys not found" });
            }

            var keysDTO = _mapper.Map<IEnumerable<KeyDTO>>(keys);

            return Ok(keysDTO);
        }

    }
}