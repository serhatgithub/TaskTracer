using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskTracer.TaskService.Data;
using TaskTracer.TaskService.Models;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace TaskTracer.TaskService.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly TaskDbContext _ctx;
        private readonly ILogger<TasksController> _logger;

        public TasksController(TaskDbContext ctx, ILogger<TasksController> logger)
        {
            _ctx = ctx;
            _logger = logger;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var userId = GetUserId();
            var tasks = await _ctx.Tasks
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.Status)
                .ThenBy(t => t.Order)
                .Select(t => new { t.Id, t.Title, t.Status, t.Order })
                .ToListAsync();
            return Ok(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(TaskCreateDto dto)
        {
            var userId = GetUserId();
            var order = await _ctx.Tasks.CountAsync(t => t.UserId == userId && t.Status == dto.Status);
            var task = new TaskItem
            {
                Title = dto.Title,
                Status = dto.Status,
                Order = order,
                UserId = userId
            };
            _ctx.Tasks.Add(task);
            await _ctx.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, task);
        }

        [HttpPatch("{id}/order")]
        public async Task<IActionResult> ChangeTaskOrder(int id, [FromQuery(Name= "dir")] string direction)
        {
            _logger.LogInformation("ChangeTaskOrder çağrıldı. Task ID: {TaskId}, Yön: {Direction}", id, direction);

            var userId = GetUserId();
            if (userId == 0)
            {
                _logger.LogWarning("Yetkisiz erişim denemesi: Kullanıcı ID'si 0.");
                return Unauthorized();
            }
            _logger.LogInformation("Kullanıcı ID: {UserId}", userId);

            var taskToMove = await _ctx.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (taskToMove == null)
            {
                _logger.LogWarning("Görev bulunamadı. Task ID: {TaskId}", id);
                return NotFound();
            }
            _logger.LogInformation("Taşınacak görev bulundu: '{TaskTitle}', Mevcut Sıra: {Order}", taskToMove.Title, taskToMove.Order);

            if (direction.ToLower() != "up" && direction.ToLower() != "down")
            {
                _logger.LogWarning("Geçersiz yön belirtildi: {Direction}", direction);
                return BadRequest(new { message = "Invalid direction." });
            }

            _logger.LogInformation("Komşu görev aranıyor...");
            var siblingTask = direction.ToLower() == "up"
                ? await _ctx.Tasks
                    .Where(t => t.UserId == userId && t.Status == taskToMove.Status && t.Order < taskToMove.Order)
                    .OrderByDescending(t => t.Order)
                    .FirstOrDefaultAsync()
                : await _ctx.Tasks
                    .Where(t => t.UserId == userId && t.Status == taskToMove.Status && t.Order > taskToMove.Order)
                    .OrderBy(t => t.Order)
                    .FirstOrDefaultAsync();

            if (siblingTask == null)
            {
                _logger.LogWarning("Komşu görev bulunamadı. Taşıma mümkün değil. Yön: {Direction}", direction);
                return BadRequest(new
                {
                    message = direction.ToLower() == "up"
                        ? "Görev zaten en üstte."
                        : "Görev zaten en altta."
                });
            }
            _logger.LogInformation("Komşu görev bulundu: '{SiblingTaskTitle}', Sırası: {SiblingOrder}",
                siblingTask.Title, siblingTask.Order);

            // Swap order
            var originalOrder = taskToMove.Order;
            taskToMove.Order = siblingTask.Order;
            siblingTask.Order = originalOrder;
            _logger.LogInformation("Sıralar takas edildi. Yeni sıra: {NewOrder}, Komşunun yeni sırası: {SiblingNewOrder}",
                taskToMove.Order, siblingTask.Order);

            try
            {
                await _ctx.SaveChangesAsync();
                _logger.LogInformation("Değişiklikler veritabanına başarıyla kaydedildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveChangesAsync sırasında bir hata oluştu.");
                return StatusCode(500, "Veritabanı kaydı sırasında bir hata oluştu.");
            }

            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeTaskStatus(int id, [FromQuery(Name = "to")] Models.TaskStatus newStatus)
        {
            var userId = GetUserId();
            var taskToMove = await _ctx.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (taskToMove == null) return NotFound();
            if (taskToMove.Status == newStatus) return NoContent();

            var oldStatus = taskToMove.Status;
            var oldOrder = taskToMove.Order;

            var tasksToReorderInOldList = await _ctx.Tasks
                .Where(t => t.UserId == userId && t.Status == oldStatus && t.Order > oldOrder)
                .ToListAsync();
            foreach (var t in tasksToReorderInOldList) t.Order--;

            var newOrder = await _ctx.Tasks.CountAsync(t => t.UserId == userId && t.Status == newStatus);
            taskToMove.Status = newStatus;
            taskToMove.Order = newOrder;

            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetUserId();
            var taskToDelete = await _ctx.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (taskToDelete == null) return NotFound();

            var oldStatus = taskToDelete.Status;
            var oldOrder = taskToDelete.Order;

            _ctx.Tasks.Remove(taskToDelete);

            var tasksToReorder = await _ctx.Tasks
                .Where(t => t.UserId == userId && t.Status == oldStatus && t.Order > oldOrder)
                .ToListAsync();
            foreach (var t in tasksToReorder) t.Order--;

            await _ctx.SaveChangesAsync();
            return NoContent();
        }
    }
}
