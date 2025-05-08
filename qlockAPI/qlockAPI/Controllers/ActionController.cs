using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using qlockAPI.Core.Database;
using qlockAPI.Core.DTOs.ActionDTOs;
using qlockAPI.Core.DTOs.NotificationDTOs;
using qlockAPI.Core.DTOs.UserDTOs;
using qlockAPI.Core.Entities;
using qlockAPI.Core.Services.KeyService;
using qlockAPI.Core.Services.LogService;
using qlockAPI.Notification;

namespace qlockAPI.Controllers
{
    //[ApiKeyAuthorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ActionController : ControllerBase
    {
        private readonly QlockContext _context;
        private readonly IMapper _mapper;
        private readonly IKeyService _keyService;
        private readonly ILogService _logService;
        private readonly IPushNotificationService _pushNotificationService;

        public ActionController(QlockContext context,IMapper mapper,IKeyService keyService,ILogService logService,IPushNotificationService pushNotificationService)
        {
            _context = context;
            _mapper = mapper;
            _keyService = keyService;
            _logService = logService;
            _pushNotificationService = pushNotificationService;
        }

        [HttpPost]
        [Route("open")]
        public async Task<IActionResult> OpenLock([FromBody] LockOpenRequestDTO request)
        {
            Console.WriteLine("Open api meghíva.");
            if (request is null || request.KeyId <= 0 || request.UserId <= 0 || request.LockId <= 0)
            {
                return BadRequest(new { error = "Invalid request." });
            }

            var lockEntity = await _context.Locks.FindAsync(request.LockId);
            var lockOwner = await _context.Users.FirstOrDefaultAsync(u => u.Id == lockEntity.Owner);

            async Task SendPushAndLog(string title, string body, string status, string text)
            {
                Console.WriteLine("Text: " + text + "body:" + body);
                if (lockOwner?.Pushtoken is not null)
                {
                    var pushRequest = new PushRequest
                    {
                        Token = lockOwner.Pushtoken,
                        Title = title,
                        Body = body
                    };

                    var sent = await _pushNotificationService.SendPushNotificationAsync(pushRequest);
                    if (sent)
                    {
                        await _logService.LogActionAsync(request.UserId, request.LockId, request.KeyId, request.RequestTime, "PushNotification", "Success", $"{title} - {body} - {text}");
                    }
                }

                await _logService.LogActionAsync(request.UserId, request.LockId, request.KeyId, request.RequestTime, "OpenLock", status, text);
            }

            var isValidKey = await _keyService.IsKeyValidAsync(request.KeyId);
            if (!isValidKey)
            {
                await SendPushAndLog("Warning!", "Failed access attempt detected on your lock.", "Failed", "Key is invalid or inactive.");
                return BadRequest(new { error = "Key is invalid or inactive." });
            }

            var isAssigned = await _keyService.IsKeyAssignedToLockAsync(request.KeyId, request.LockId);
            if (!isAssigned)
            {
                await SendPushAndLog("Warning!", "Failed access attempt detected on your lock.", "Failed", "Key is not assigned to the specified lock.");
                return BadRequest(new { error = "Key is not assigned to the specified lock." });
            }

            var isKeyForUser = await _keyService.IsKeyAssignedToUserAsync(request.KeyId, request.UserId);
            if (!isKeyForUser)
            {
                return BadRequest(new { error = "Key does not belong to the specified user." });
            }

            await SendPushAndLog("Opening attempt!", "Someone has accessed your lock.", "Success", "Key validated successfully. Awaiting lock confirmation.");


            await _keyService.DecreaseKeyUsageAsync(request.KeyId);
            
            return Ok(new { status = "ok", message = "Key validated. Waiting for lock confirmation for action to proceed." });
        }


        [HttpPost]
        [Route("confrim")]
        public async Task<IActionResult> ConfirmLockAction([FromBody] LockConfirmRequestDTO request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                bool isDecreased = await _keyService.DecreaseKeyUsageAsync(request.KeyId);
                bool isValidated = await _keyService.ValidateKeyAsync(request.KeyId);

                if (!isDecreased || !isValidated)
                {
                    throw new InvalidOperationException("Key validation or decrement failed.");
                }

                _context.Logs.Add(new Log
                {
                    KeyId = request.KeyId,
                    LockId = request.LockId,
                    UserId = request.UserId,
                    Action = "open",
                    Status = request.Status,
                    Time = DateTime.Now
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Open request logged." });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { error = "Internal server error." });
            }
        }


    }
}
