using Microsoft.EntityFrameworkCore;
using RustyMangoApi.Models;
using RustyMangoAPI.Models;

namespace RustyMangoAPI.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserProgress> UserProgresses { get; set; }
    }
}
