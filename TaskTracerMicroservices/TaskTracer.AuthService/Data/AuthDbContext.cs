using Microsoft.EntityFrameworkCore;
using TaskTracer.AuthService.Models; // Model namespace'ini güncelledik

namespace TaskTracer.AuthService.Data // Namespace'i güncelledik
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Kullanıcı adı benzersiz olmalı (opsiyonel ama önerilir)
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}