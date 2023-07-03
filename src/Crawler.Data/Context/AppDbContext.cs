using Crawler.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crawler.Data.Context;

public sealed class AppDbContext : DbContext
{
    public DbSet<Link> Links { get; set; } = default!;
    public DbSet<PageDatum> PageData { get; set; } = default!;
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Link>(p => p.HasIndex(c => c.Url).IsUnique());
    }
}