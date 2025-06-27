using Microsoft.EntityFrameworkCore;
using TaskTracer.TaskService.Models;

namespace TaskTracer.TaskService.Data
{
    public class TaskDbContext : DbContext
    {
        public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
        {
        }

        public DbSet<TaskItem> Tasks => Set<TaskItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Kullanıcıya ait görevleri ve durumlarına göre sıralamayı optimize etmek için index
            modelBuilder.Entity<TaskItem>()
                .HasIndex(t => new { t.UserId, t.Status, t.Order });
        }
    }
}