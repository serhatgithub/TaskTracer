namespace TaskTracer.TaskService.Models
{
    // Enum'ı doğrudan DTO içinde kullanabiliriz.
    public record TaskCreateDto(string Title, TaskStatus Status);
}