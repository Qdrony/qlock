using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using qlockAPI.Core.Database;
using qlockAPI.Core.DTOs.ActionDTOs;
using qlockAPI.Core.DTOs.UserDTOs;
using qlockAPI.Core.Entities;
using qlockAPI.Core.Services.KeyService;

namespace qlockAPI.Controllers
{
    [ApiKeyAuthorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ActionController : ControllerBase
    {
        private readonly QlockContext _context;
        private readonly IMapper _mapper;
        private readonly IKeyService _keyService;

        public ActionController(QlockContext context,IMapper mapper,IKeyService keyService)
        {
            _context = context;
            _mapper = mapper;
            _keyService = keyService;
        }

        [HttpPost]
        [Route("open")]
        public async Task<IActionResult> OpenLock([FromBody] LockOpenRequestDTO request)
        {
            if (request is null || request.KeyId <= 0 || request.UserId <= 0 || request.LockId <= 0)
            {
                return BadRequest(new { error = "Invalid request." });
            }

            var isValidKey = await _keyService.IsKeyValidAsync(request.KeyId);
            if (!isValidKey)
            {
                return BadRequest(new { error = "Key is invalid or inactive." });
            }

            var isAssigned = await _keyService.IsKeyAssignedToLockAsync(request.KeyId, request.LockId);
            if (!isAssigned)
            {
                return BadRequest(new { error = "Key is not assigned to the specified lock." });
            }

            var isKeyForUser = await _keyService.IsKeyAssignedToUserAsync(request.KeyId, request.UserId);
            if (!isKeyForUser)
            {
                return BadRequest(new { error = "Key does not belong to the specified user." });
            }

            var keyEntity = await _context.Keys.AsNoTracking().FirstOrDefaultAsync(k => k.Id == request.KeyId);

            return Ok(new { secretkey = keyEntity.SecretKey, message = "Key validated. Waiting for lock confirmation for action to proceed." });
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
