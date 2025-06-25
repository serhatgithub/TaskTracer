namespace TaskTracer.Api.Models;

public class TaskItem
{
    public int Id       { get; set; }
    public string Title { get; set; } = null!;
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public int Order    { get; set; } = 0;      // sütun içindeki sırası
    public int UserId   { get; set; }           // ileride User tablosu eklendiğinde FK
}
