namespace TaskTracer.TaskService.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public TaskStatus Status { get; set; } = TaskStatus.Todo;
        public int Order { get; set; } = 0;      // Sütun içindeki sırası
        public int UserId { get; set; }           // Bu görevin hangi kullanıcıya ait olduğunu belirtir
    }
}