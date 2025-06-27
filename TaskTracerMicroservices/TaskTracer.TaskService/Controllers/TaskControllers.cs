using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskTracer.TaskService.Data;
using TaskTracer.TaskService.Models;
// TaskStatus enum'ı Models namespace'inde olduğu için ayrıca using'e gerek yok
// ama açıkça belirtmek isterseniz: using TaskStatus = TaskTracer.TaskService.Models.TaskStatus;


namespace TaskTracer.TaskService.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    [Authorize] // Bu controller'daki tüm endpoint'ler yetkilendirme gerektirir
    public class TasksController : ControllerBase
    {
        private readonly TaskDbContext _ctx;

        public TasksController(TaskDbContext ctx)
        {
            _ctx = ctx;
        }

        private int GetUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idStr, out var id) ? id : 0;
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks([FromQuery] Models.TaskStatus? status) // Namespace ile çakışmayı önlemek için Models.TaskStatus
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var query = _ctx.Tasks.Where(t => t.UserId == userId);

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            var result = await query
                .OrderBy(t => t.Order) // Önce Order'a göre sırala
                .Select(t => new { t.Id, t.Title, t.Status, t.Order }) // Sadece gerekli alanları seç
                .ToListAsync();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(TaskCreateDto dto)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            // Yeni görevin o statüsteki sırasını belirle
            var order = await _ctx.Tasks
                .Where(t => t.UserId == userId && t.Status == dto.Status)
                .CountAsync(); // Mevcut görev sayısını alarak son sıraya ekle

            var task = new TaskItem
            {
                Title = dto.Title,
                Status = dto.Status,
                Order = order,
                UserId = userId
            };

            _ctx.Tasks.Add(task);
            await _ctx.SaveChangesAsync();

            // Oluşturulan kaynağın detaylarını dönmek iyi bir pratiktir.
            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, 
                new { task.Id, task.Title, task.Status, task.Order, task.UserId });
        }

        // Tek bir görevi ID ile getirmek için bir endpoint (CreatedAtAction için gerekli)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var task = await _ctx.Tasks
                                 .Where(t => t.Id == id && t.UserId == userId)
                                 .Select(t => new { t.Id, t.Title, t.Status, t.Order, t.UserId })
                                 .FirstOrDefaultAsync();

            if (task == null)
            {
                return NotFound();
            }
            return Ok(task);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var task = await _ctx.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (task == null)
            {
                return NotFound();
            }

            _ctx.Tasks.Remove(task);

            // Silme işleminden sonra aynı statüdeki diğer görevlerin order'larını güncelle
            var tasksToReorder = await _ctx.Tasks
                .Where(t => t.UserId == userId && t.Status == task.Status && t.Order > task.Order)
                .OrderBy(t => t.Order)
                .ToListAsync();

            foreach (var ttr in tasksToReorder)
            {
                ttr.Order--;
            }

            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/order")]
        public async Task<IActionResult> ChangeTaskOrder(int id, [FromQuery] string direction) // "up" or "down"
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var task = await _ctx.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (task == null)
            {
                return NotFound();
            }

            var list = await _ctx.Tasks
                .Where(t => t.Status == task.Status && t.UserId == userId)
                .OrderBy(t => t.Order)
                .ToListAsync();

            var currentIndex = list.FindIndex(t => t.Id == id);

            if (direction.ToLower() == "up")
            {
                if (currentIndex > 0)
                {
                    // Swap order with the task above
                    var taskAbove = list[currentIndex - 1];
                    (task.Order, taskAbove.Order) = (taskAbove.Order, task.Order);
                }
                else
                {
                    return BadRequest(new { message = "Cannot move task further up." });
                }
            }
            else if (direction.ToLower() == "down")
            {
                if (currentIndex < list.Count - 1)
                {
                    // Swap order with the task below
                    var taskBelow = list[currentIndex + 1];
                    (task.Order, taskBelow.Order) = (taskBelow.Order, task.Order);
                }
                else
                {
                    return BadRequest(new { message = "Cannot move task further down." });
                }
            }
            else
            {
                return BadRequest(new { message = "Invalid direction. Use 'up' or 'down'." });
            }

            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeTaskStatus(int id, [FromQuery] Models.TaskStatus newStatus) // enum'ı query'den alıyoruz
        {
            var userId = GetUserId();
            if (userId == 0) return Unauthorized();

            var task = await _ctx.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (task == null)
            {
                return NotFound();
            }

            if (task.Status == newStatus) // Durum zaten aynıysa bir şey yapma
            {
                return NoContent();
            }

            var oldStatus = task.Status;

            // Eski listedeki order'ı > olanları bir azalt
            var tasksToReorderInOldList = await _ctx.Tasks
                .Where(t => t.UserId == userId && t.Status == oldStatus && t.Order > task.Order)
                .ToListAsync();
            foreach(var ttr in tasksToReorderInOldList)
            {
                ttr.Order--;
            }

            // Yeni listedeki yerini belirle (sona ekle)
            var newOrderInNewList = await _ctx.Tasks
                .Where(t => t.UserId == userId && t.Status == newStatus)
                .CountAsync();

            task.Status = newStatus;
            task.Order = newOrderInNewList;

            await _ctx.SaveChangesAsync();
            return NoContent();
        }
    }
}