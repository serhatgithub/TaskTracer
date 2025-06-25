using Microsoft.EntityFrameworkCore;
using TaskTracer.Api.Models;

namespace TaskTracer.Api.Data;

public class TaskTrackerContext : DbContext
{
    public TaskTrackerContext(DbContextOptions<TaskTrackerContext> opt) : base(opt) { }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<User> Users => Set<User>();


    protected override void OnModelCreating(ModelBuilder b)
    {
        // Status + Order ikilisine indeks (liste sıralama performansı)
        b.Entity<TaskItem>()
         .HasIndex(t => new { t.Status, t.Order });
    }
}
