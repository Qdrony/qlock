using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qlockAPI.Core.Database;
using qlockAPI.Core.DTOs.ActionDTOs;
using qlockAPI.Core.DTOs.NotificationDTOs;
using qlockAPI.Core.Entities;
using qlockAPI.Core.Services.KeyService;
using qlockAPI.Core.Services.LockService;
using qlockAPI.Core.Services.LogService;
using qlockAPI.Core.Services.MonitorService;
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
        private readonly ILockAttemptMonitor _lockAttemptMonitor;
        private readonly ILockService _lockService;

        public ActionController(QlockContext context,IMapper mapper,IKeyService keyService,ILogService logService,IPushNotificationService pushNotificationService,ILockAttemptMonitor lockAttemptMonitor,ILockService lockService)
        {
            _context = context;
            _mapper = mapper;
            _keyService = keyService;
            _logService = logService;
            _pushNotificationService = pushNotificationService;
            _lockAttemptMonitor = lockAttemptMonitor;
            _lockService = lockService;
        }

        [HttpPost]
        [Route("open")]
        public async Task<IActionResult> OpenLock([FromBody] LockOpenRequestDTO request)
        {
            if (request is null || request.KeyId <= 0 || request.UserId <= 0 || request.LockId <= 0)
            {
                return BadRequest(new { error = "Invalid request." });
            }

            var lockEntity = await _context.Locks.FindAsync(request.LockId);
            var lockOwner = await _context.Users.FirstOrDefaultAsync(u => u.Id == lockEntity.Owner);
            var requestUserPushtoken = _context.Users.FirstOrDefault(u => u.Id == request.UserId)?.Pushtoken;

            async Task SendPush(string pushToken,string title, string body)
            {
                if (lockOwner?.Pushtoken is not null)
                {
                    var pushRequest = new PushRequest
                    {
                        Token = pushToken,
                        Title = title,
                        Body = body
                    };

                    await _pushNotificationService.SendPushNotificationAsync(pushRequest);
                }
            }

            var isValidKey = await _keyService.IsKeyValidAsync(request.KeyId);
            if (!isValidKey)
            {
                if (_lockAttemptMonitor.RegisterFailedAttemptAsync(request.LockId).Result) 
                {
                    _lockService.BlockLockAsync(request.LockId).Wait();
                    await SendPush(lockOwner.Pushtoken, "Warning!", "Your lock has been temporarily disabled due to repeated failed access attempts.");
                }
                await SendPush(lockOwner.Pushtoken, "Warning!", "Failed access attempt detected on your lock. Key is invalid or inactive.");
                await SendPush(requestUserPushtoken, "Warning!", "Failed access attempt. Key is invalid or inactive.");
                await _logService.LogActionAsync(request.UserId, request.LockId, request.KeyId, request.RequestTime, "OpenLock", "Failed", "Key is invalid or inactive");
                return BadRequest(new { error = "Key is invalid or inactive." });
            }

            var isAssigned = await _keyService.IsKeyAssignedToLockAsync(request.KeyId, request.LockId);
            if (!isAssigned)
            {
                await SendPush(lockOwner.Pushtoken,"Warning!", "Failed access attempt detected on your lock. Key is not assigned to the specified lock.");
                await SendPush(requestUserPushtoken, "Warning!", "Failed access attempt. Key is invalid or inactive.");
                await _logService.LogActionAsync(request.UserId, request.LockId, request.KeyId, request.RequestTime, "OpenLock", "Failed", "Key is not assigned to the specified lock");
                return BadRequest(new { error = "Key is not assigned to the specified lock." });
            }

            var isKeyForUser = await _keyService.IsKeyAssignedToUserAsync(request.KeyId, request.UserId);
            if (!isKeyForUser)
            {
                return BadRequest(new { error = "Key does not belong to the specified user." });
            }

            await SendPush(lockOwner.Pushtoken, "Opening attempt!", "Someone has accessed your lock. Key validated successfully.");  
            await SendPush(requestUserPushtoken,"Opening attempt!", "Successful entry");

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
