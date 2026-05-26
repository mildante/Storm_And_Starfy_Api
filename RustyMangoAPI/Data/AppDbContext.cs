using Microsoft.EntityFrameworkCore;
using StormAndStarfyApi.Models;

namespace StormAndStarfyApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(user => user.Login).IsUnique();
                entity.Property(user => user.Login).IsRequired();
                entity.Property(user => user.Name).IsRequired();
                entity.Property(user => user.PasswordHash).IsRequired();
                entity.Property(user => user.CreatedAtUtc).IsRequired();
            });
        }
    }
}
