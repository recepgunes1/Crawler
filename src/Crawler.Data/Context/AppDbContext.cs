using Crawler.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crawler.Data.Context;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Link> Links { get; set; } = default!;
    public DbSet<PageDatum> PageData { get; set; } = default!;
    public DbSet<Book> Books { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Link>(p => p.HasIndex(c => c.Url).IsUnique());
    }
}