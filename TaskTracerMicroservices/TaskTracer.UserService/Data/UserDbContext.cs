using Microsoft.EntityFrameworkCore;
using TaskTracer.UserService.Models;

namespace TaskTracer.UserService.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // AuthDbContext'te zaten tanımlı olduğu için burada tekrar tanımlamak
            // migration çakışmalarına neden olabilir. Eğer aynı DB kullanılıyorsa
            // bu index tanımı burada olmasa da olur.
            // Farklı DB context'leri aynı tabloya farklı yapılarla erişmeye çalışırsa sorun olur.
            // Şimdilik, bu servisin kendi migration'larını yönetmeyeceğini varsayarak
            // bu kısmı yoruma alabiliriz veya bırakabiliriz, çünkü AuthDbContext zaten bu index'i oluşturuyor olmalı.
            /*
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            */
        }
    }
}