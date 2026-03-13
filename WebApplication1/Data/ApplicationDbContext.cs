using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<GameHistory> GameHistories { get; set; }
        public DbSet<UserHighScore> UserHighScores { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<GameHistory>().ToTable("GameHistories");
            builder.Entity<UserHighScore>().ToTable("UserHighScores");
        }


    }
}
