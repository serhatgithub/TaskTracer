using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using TaskTracer.Api.Data;
using TaskTracer.Api.Models;
using TaskTracer.Api.Services;

namespace TaskTracer.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly TaskTrackerContext _ctx;
    private readonly TokenService _tok;

    public AuthController(TaskTrackerContext ctx, TokenService tok)
    {
        _ctx = ctx;
        _tok = tok;
    }

    // DTO’lar
    public record RegisterDto(string Username, string Password);
    public record LoginDto(string Username, string Password);
    public record PasswordUpdateRequest(string OldPassword, string NewPassword);
    public record UsernameUpdateRequest(string NewUsername);

    // Kullanıcı Kaydı
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _ctx.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest("username_exists");

        CreateHash(dto.Password, out var hash, out var salt);

        _ctx.Users.Add(new User
        {
            Username = dto.Username,
            PasswordHash = hash,
            PasswordSalt = salt
        });

        await _ctx.SaveChangesAsync();
        return StatusCode(201);
    }

    // Kullanıcı Girişi
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _ctx.Users.SingleOrDefaultAsync(u => u.Username == dto.Username);
        if (user is null || !Verify(dto.Password, user))
            return Unauthorized("invalid_credentials");

        var token = _tok.CreateToken(user);
        return Ok(new { token });
    }

    // 🔒 Şifre Değiştirme
    [Authorize]
    [HttpPatch("password")]
    public async Task<IActionResult> ChangePassword([FromBody] PasswordUpdateRequest req)
    {
        var userId = GetUserId();
        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return NotFound();

        if (!Verify(req.OldPassword, user))
            return BadRequest("wrong_password");

        CreateHash(req.NewPassword, out var hash, out var salt);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;

        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    // 👤 Kullanıcı Adı Değiştirme
    [Authorize]
    [HttpPatch("username")]
    public async Task<IActionResult> ChangeUsername([FromBody] UsernameUpdateRequest req)
    {
        var userId = GetUserId();
        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return NotFound();

        var exists = await _ctx.Users.AnyAsync(u => u.Username == req.NewUsername);
        if (exists) return BadRequest("username_exists");

        user.Username = req.NewUsername;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    // Yardımcı Fonksiyonlar
    private static void CreateHash(string pw, out byte[] hash, out byte[] salt)
    {
        using var h = new HMACSHA512();
        salt = h.Key;
        hash = h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pw));
    }

    private static bool Verify(string pw, User u)
    {
        using var h = new HMACSHA512(u.PasswordSalt);
        var comp = h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(pw));
        return comp.SequenceEqual(u.PasswordHash);
    }

    private int GetUserId()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idStr, out var id) ? id : 0;
    }
}
