using Microsoft.EntityFrameworkCore;

namespace SwapSoon.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();

    public DbSet<Replacement> Replacements => Set<Replacement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>(item =>
        {
            item.Property(i => i.Name)
                .IsRequired()
                .HasMaxLength(100);

            item.HasMany(i => i.Replacements)
                .WithOne(r => r.Item)
                .HasForeignKey(r => r.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
