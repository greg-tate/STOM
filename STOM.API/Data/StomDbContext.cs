using Microsoft.EntityFrameworkCore;
using STOM.API.Models;

namespace STOM.API.Data;

public class StomDbContext : DbContext
{
    public StomDbContext(DbContextOptions<StomDbContext> options) : base(options)
    {
    }

    public DbSet<Game> Games { get; set; }
    public DbSet<Score> Scores { get; set; }
    public DbSet<Tournament> Tournaments { get; set; }
    public DbSet<Competition> Competitions { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>()
            .HasMany(g => g.Scores)
            .WithOne(s => s.Game)
            .HasForeignKey(s => s.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tournament>()
            .HasMany(t => t.Games)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Competition>()
            .HasMany(c => c.Games)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.Timestamp)
            .IsDescending();

        modelBuilder.Entity<AuditLog>()
            .HasIndex(a => a.PerformedBy);
    }
}
