using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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
    private readonly PasswordService _password;

    public AuthController(TaskTrackerContext ctx, TokenService tok, PasswordService password)
    {
        _ctx = ctx;
        _tok = tok;
        _password = password;
    }

    public record RegisterDto(string Username, string Password);
    public record LoginDto(string Username, string Password);
    public record PasswordUpdateRequest(string OldPassword, string NewPassword);
    public record UsernameUpdateRequest(string NewUsername);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _ctx.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest(new { error = "username_exists" });

        _password.CreateHash(dto.Password, out var hash, out var salt);

        _ctx.Users.Add(new User
        {
            Username = dto.Username,
            PasswordHash = hash,
            PasswordSalt = salt
        });

        await _ctx.SaveChangesAsync();
        return StatusCode(201);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _ctx.Users.SingleOrDefaultAsync(u => u.Username == dto.Username);
        if (user is null || !_password.Verify(dto.Password, user.PasswordHash, user.PasswordSalt))
            return Unauthorized(new { error = "invalid_credentials" });

        var token = _tok.CreateToken(user);
        return Ok(new { token });
    }

    [Authorize]
    [HttpPatch("password")]
    public async Task<IActionResult> ChangePassword([FromBody] PasswordUpdateRequest req)
    {
        var userId = GetUserId();
        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return NotFound();

        if (!_password.Verify(req.OldPassword, user.PasswordHash, user.PasswordSalt))
            return BadRequest(new { error = "wrong_password" });

        _password.CreateHash(req.NewPassword, out var hash, out var salt);
        user.PasswordHash = hash;
        user.PasswordSalt = salt;

        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [Authorize]
    [HttpPatch("username")]
    public async Task<IActionResult> ChangeUsername([FromBody] UsernameUpdateRequest req)
    {
        var userId = GetUserId();
        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return NotFound();

        var exists = await _ctx.Users.AnyAsync(u => u.Username == req.NewUsername && u.Id != userId);
        if (exists) return BadRequest(new { error = "username_exists" });

        user.Username = req.NewUsername;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    private int GetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idStr, out var id) ? id : 0;
    }
}
