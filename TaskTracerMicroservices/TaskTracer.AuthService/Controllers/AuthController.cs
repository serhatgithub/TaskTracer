using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracer.AuthService.Data;       // Data namespace'ini güncelledik
using TaskTracer.AuthService.Models;    // Models namespace'ini güncelledik
using TaskTracer.AuthService.Services;  // Services namespace'ini güncelledik

namespace TaskTracer.AuthService.Controllers // Namespace'i güncelledik
{
    [ApiController]
    [Route("api/auth")] // API route'unu "api/auth" olarak değiştirebiliriz.
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _ctx;
        private readonly TokenService _tokenService;
        private readonly PasswordService _passwordService;

        public AuthController(AuthDbContext ctx, TokenService tokenService, PasswordService passwordService)
        {
            _ctx = ctx;
            _tokenService = tokenService;
            _passwordService = passwordService;
        }

        // DTO'lar zaten Models klasöründe tanımlı olduğu için burada tekrar tanımlamaya gerek yok.
        // using TaskTracer.AuthService.Models; ile erişilebilirler.

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _ctx.Users.AnyAsync(u => u.Username == dto.Username))
            {
                // Kullanıcı adı zaten mevcutsa BadRequest dönüyoruz.
                // Daha detaylı hata mesajları eklenebilir.
                return BadRequest(new { message = "Username already exists." });
            }

            _passwordService.CreateHash(dto.Password, out var passwordHash, out var passwordSalt);

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            _ctx.Users.Add(user);
            await _ctx.SaveChangesAsync();

            // Kayıt başarılı olduğunda 201 Created durum kodu ve belki de kullanıcı ID'si dönebiliriz.
            // Şimdilik sadece 201 yeterli.
            return StatusCode(201, new { message = "User registered successfully."});
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _ctx.Users.SingleOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !_passwordService.Verify(dto.Password, user.PasswordHash, user.PasswordSalt))
            {
                // Kullanıcı bulunamazsa veya şifre yanlışsa Unauthorized dönüyoruz.
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var token = _tokenService.CreateToken(user);

            return Ok(new { token });
        }
    }
}