using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using UserManagementSystem.Models;

namespace UserManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ВАЖНО: Создание уникального индекса для Email
            // Это гарантирует уникальность на уровне базы данных
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Note: Это не проверка в коде, а ограничение на уровне БД
            // База данных сама отклонит дубликаты

            // Можно добавить индекс для оптимизации сортировки
            modelBuilder.Entity<User>()
                .HasIndex(u => u.LastLoginTime);
        }
    }
}