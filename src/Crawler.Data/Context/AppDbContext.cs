using Crawler.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crawler.Data.Context;

public class AppDbContext : DbContext
{
    public DbSet<Link> Links { get; set; } = default!;
    public DbSet<PageDatum> PageData { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql("Host=s_database;Port=5432;Database=crawler;Username=postgres;Password=Password123.");
    }
}