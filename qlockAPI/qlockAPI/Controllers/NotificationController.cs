using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qlockAPI.Core.Database;
using qlockAPI.Core.DTOs.NotificationDTOs;
using qlockAPI.Notification;

namespace qlockAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IPushNotificationService _pushNotificationService;
        private readonly QlockContext _context;

        public NotificationController(IPushNotificationService pushNotificationService, QlockContext context)
        {
            _pushNotificationService = pushNotificationService;
            _context = context;
        }

        [HttpPost("register-token")]
        public async Task<IActionResult> RegisterPushToken([FromBody] PushTokenDTO pushTokenDTO)
        {
            var user = await _context.Users.FindAsync(pushTokenDTO.UserId);
            if (user is null)
            {
                return NotFound(new { error = "User not found" });
            }

            user.Pushtoken = pushTokenDTO.Token;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Push token registered" });
        }
    }
}
