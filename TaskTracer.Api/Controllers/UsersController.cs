using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskTracer.Api.Data;
using TaskTracer.Api.Models;
using TaskTracer.Api.Services;

namespace TaskTracer.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly TaskTrackerContext _ctx;
    private readonly PasswordService _passwordService;

    public UsersController(TaskTrackerContext ctx, PasswordService passwordService)
    {
        _ctx = ctx;
        _passwordService = passwordService;
    }

    private int GetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idStr, out var id) ? id : 0;
    }

    [HttpPatch("username")]
    public async Task<IActionResult> ChangeUsername([FromBody] string newUsername)
    {
        var userId = GetUserId();
        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return NotFound();

        // Aynı username'den varsa hata ver
        var exists = await _ctx.Users.AnyAsync(u => u.Username == newUsername && u.Id != userId);
        if (exists)
            return BadRequest(new { error = "username_taken" });

        user.Username = newUsername;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("password")]
    public async Task<IActionResult> ChangePassword([FromBody] PasswordUpdateRequest req)
    {
        var userId = GetUserId();
        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return NotFound();

        var isMatch = _passwordService.Verify(req.OldPassword, user.PasswordHash, user.PasswordSalt);
        if (!isMatch)
            return BadRequest(new { error = "wrong_password" });

        // Yeni şifre için hash ve salt üret
        _passwordService.CreateHash(req.NewPassword, out var newHash, out var newSalt);
        user.PasswordHash = newHash;
        user.PasswordSalt = newSalt;

        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUserId();
        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return NotFound();

        return Ok(new { username = user.Username });
    }

    public class PasswordUpdateRequest
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
