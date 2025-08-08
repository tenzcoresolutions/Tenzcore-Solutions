using ApiWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiWeb.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Vote> Votes => Set<Vote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Message>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Text).IsRequired().HasMaxLength(300);
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.Upvotes).HasDefaultValue(0);
            b.Property(x => x.Downvotes).HasDefaultValue(0);
            b.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<Vote>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.VoterHash).IsRequired();
            b.Property(x => x.Value).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasOne(x => x.Message)
                .WithMany()
                .HasForeignKey(x => x.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.MessageId, x.VoterHash }).IsUnique();
        });
    }
}