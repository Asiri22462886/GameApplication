using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WordGame.Models;

namespace WordGame.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<GameHistory> GameHistories { get; set; }
        public DbSet<UserHighScore> UserHighScores { get; set; }
        // Passes the database options into the Identity database context.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }
        // Adds the game specific tables while keeping the default Identity tables.
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<GameHistory>().ToTable("GameHistories");
            builder.Entity<UserHighScore>().ToTable("UserHighScores");
        }


    }
}
