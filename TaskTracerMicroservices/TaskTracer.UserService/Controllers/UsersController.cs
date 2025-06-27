using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskTracer.UserService.Data;
using TaskTracer.UserService.Models;
using TaskTracer.UserService.Services;

namespace TaskTracer.UserService.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize] // Bu controller'daki tüm endpoint'ler yetkilendirme gerektirir
    public class UsersController : ControllerBase
    {
        private readonly UserDbContext _ctx;
        private readonly PasswordService _passwordService;

        public UsersController(UserDbContext ctx, PasswordService passwordService)
        {
            _ctx = ctx;
            _passwordService = passwordService;
        }

        private int GetUserId()
        {
            // Token'dan NameIdentifier claim'ini (kullanıcı ID'si) al
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out var id) ? id : 0;
        }

        [HttpPatch("username")]
        public async Task<IActionResult> ChangeUsername([FromBody] UsernameUpdateRequest req) // DTO'yu body'den alıyoruz
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized(); // Geçerli bir kullanıcı ID'si yoksa

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound(new { message = "User not found." });

            // Yeni kullanıcı adının başkası tarafından kullanılıp kullanılmadığını kontrol et
            var exists = await _ctx.Users.AnyAsync(u => u.Username == req.NewUsername && u.Id != userId);
            if (exists)
            {
                return BadRequest(new { message = "Username already taken." });
            }

            user.Username = req.NewUsername;
            await _ctx.SaveChangesAsync();
            return NoContent(); // Başarılı güncelleme
        }

        [HttpPatch("password")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordUpdateRequest req)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound(new { message = "User not found." });

            if (!_passwordService.Verify(req.OldPassword, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest(new { message = "Wrong old password." });
            }

            _passwordService.CreateHash(req.NewPassword, out var newHash, out var newSalt);
            user.PasswordHash = newHash;
            user.PasswordSalt = newSalt;

            await _ctx.SaveChangesAsync();
            return NoContent(); // Başarılı güncelleme
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var user = await _ctx.Users.Select(u => new { u.Id, u.Username }) // Sadece ID ve Username seçiyoruz
                                   .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound(new { message = "User not found." });

            return Ok(new { username = user.Username }); // Sadece username dönüyoruz
        }
    }
}