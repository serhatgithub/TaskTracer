using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskTracer.Api.Data;
using TaskTracer.Api.Models;
using TaskStatus = TaskTracer.Api.Models.TaskStatus;


namespace TaskTracer.Api.Controllers;

[ApiController]
[Route("api/v1/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly TaskTrackerContext _ctx;
    public TasksController(TaskTrackerContext ctx) => _ctx = ctx;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] TaskTracer.Api.Models.TaskStatus? status)
    {
        var userId = GetUserId();
        Console.WriteLine(">> userId: " + userId);
        var query = _ctx.Tasks.Where(t => t.UserId == userId);

        if (status.HasValue)
            query = query.Where(t => t.Status == status);

        var result = await query
            .OrderBy(t => t.Order)
            .Select(t => new { t.Id, t.Title, t.Status, t.Order })
            .ToListAsync();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TaskCreateDto dto)
    {
        var userId = GetUserId();
        var order = await _ctx.Tasks
            .Where(t => t.UserId == userId && t.Status == dto.Status)
            .CountAsync();

        var task = new TaskItem
        {
            Title = dto.Title,
            Status = dto.Status,
            Order = order,
            UserId = userId
        };

        _ctx.Tasks.Add(task);
        await _ctx.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var task = await _ctx.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (task is null)
            return NotFound();

        _ctx.Tasks.Remove(task);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/order")]
    public async Task<IActionResult> ChangeOrder(int id, [FromQuery] string dir)
    {
        var userId = GetUserId();
        var task = await _ctx.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (task is null)
            return NotFound();

        var list = await _ctx.Tasks
            .Where(t => t.Status == task.Status && t.UserId == userId)
            .OrderBy(t => t.Order)
            .ToListAsync();

        var index = list.FindIndex(t => t.Id == id);
        if (dir == "up" && index > 0)
        {
            (list[index - 1].Order, list[index].Order) = (list[index].Order, list[index - 1].Order);
        }
        else if (dir == "down" && index < list.Count - 1)
        {
            (list[index + 1].Order, list[index].Order) = (list[index].Order, list[index + 1].Order);
        }
        else
        {
            return BadRequest("cannot_move");
        }

        await _ctx.SaveChangesAsync();
        return NoContent();
    }

   
  [HttpPatch("{id}/status")]
public async Task<IActionResult> ChangeStatus(int id, [FromQuery] int to)
{
    var userId = GetUserId();
    var task = await _ctx.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    if (task is null)
        return NotFound();

    task.Status = (TaskStatus)to;

    var order = await _ctx.Tasks
        .Where(t => t.Status == (TaskStatus)to && t.UserId == userId)
        .CountAsync();

    task.Order = order;

    await _ctx.SaveChangesAsync();
    return NoContent();
}


    private int GetUserId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idStr, out var id) ? id : 0;
    }
}
