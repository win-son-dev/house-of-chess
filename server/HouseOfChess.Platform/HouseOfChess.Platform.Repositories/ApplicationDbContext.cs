using HouseOfChess.Platform.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace HouseOfChess.Platform.Repositories;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Game> Games => Set<Game>();
    public DbSet<GameMove> GameMoves => Set<GameMove>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.HasIndex(u => u.Auth0Sub).IsUnique();
            b.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<Game>(b =>
        {
            b.HasOne<User>()
                .WithMany()
                .HasForeignKey(g => g.WhiteUserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<User>()
                .WithMany()
                .HasForeignKey(g => g.BlackUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GameMove>(b =>
        {
            b.HasIndex(m => new { m.GameId, m.Ply }).IsUnique();

            b.HasOne<Game>()
                .WithMany()
                .HasForeignKey(m => m.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
