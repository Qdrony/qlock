using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qlockAPI.Core.Database;
using qlockAPI.Core.DTOs.KeyDTOs;
using qlockAPI.Core.Entities;
using qlockAPI.Core.Services.KeyGenerationService;

namespace qlockAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class KeyController : ControllerBase
    {
        private readonly QlockContext _context;
        private readonly IMapper _mapper;
        private readonly IKeyGenerationService _keyGenerationService;

        public KeyController(QlockContext context, IMapper mapper, IKeyGenerationService keyGenerationService)
        {
            _context = context;
            _mapper = mapper;
            _keyGenerationService = keyGenerationService;
        }

        //Create key
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateKey([FromBody] CreateKeyDTO createKeyDTO)
        {
            var user = await _context.Users.AnyAsync(u => u.Id == createKeyDTO.UserId);
            if (!user)
            {
                return NotFound(new { error = "Owner not found" });
            }

            var lockEntity = await _context.Locks.AnyAsync(l => l.Id == createKeyDTO.LockId);
            if (!lockEntity)
            {
                return NotFound(new { error = "Lock not found" });
            }

            var keyEntity = _mapper.Map<Key>(createKeyDTO);
            keyEntity.SecretKey = _keyGenerationService.GenerateSecretKey();
            keyEntity.CreatedAt = DateTime.Now;
            keyEntity.Status = "Active";

            try
            {
                await _context.Keys.AddAsync(keyEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while creating the key" });
            }

            return Ok(new { message = "Key created" });
        }

        //Key Delete
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteKey([FromRoute] int id)
        {
            var key = await _context.Keys.FindAsync(id);
            if (key is null)
            {
                return NotFound(new { error = "Key not found" });
            }

            try
            {
                _context.Keys.Remove(key);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "An error occurred while deleting the key" });
            }

            return Ok("Key deleted");
        }

        //Update key
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdateKey([FromBody] UpdateKeyDTO updateKeyDTO)
        {
            var key = await _context.Keys.FirstOrDefaultAsync(k => k.Id == updateKeyDTO.Id);

            if (key is null)
            {
                return NotFound(new { error = "Key not found" });
            }
            
            key.Status = updateKeyDTO.Status;
            key.RemainingUses = updateKeyDTO.RemainingUses;
            key.ExpirationDate = updateKeyDTO.ExpirationDate;
            key.Name = updateKeyDTO.Name;

            try
            {
                _context.Keys.Update(key);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { error = "An unexpected error occurred"});
            }

            return Ok(new { message = "Key updated successfully" });
        }

    }
}
