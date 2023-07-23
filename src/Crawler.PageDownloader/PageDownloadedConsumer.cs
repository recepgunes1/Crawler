using Crawler.Data.Context;
using Crawler.Data.Entities;
using Crawler.Shared.Configuration;
using Crawler.Shared.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Crawler.PageDownloader;

public class PageDownloadedConsumer : IConsumer<RequestedUrl>
{
    public async Task Consume(ConsumeContext<RequestedUrl> context)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT")}.json");
        var config = configuration.Build();
        var connectionString = config.GetConnectionString("postgresql");

        DbContextOptionsBuilder<AppDbContext> builder = new();
        builder.UseNpgsql(connectionString);

        AppDbContext dbContext = new(builder.Options);
        var link = await dbContext.Links.FindAsync(context.Message.Id) ?? throw new ArgumentException();

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.150 Safari/537.36");
        client.DefaultRequestHeaders.Add("Referer", link.Url);

        var sourceCode = client.GetStringAsync(link.Url).GetAwaiter().GetResult();
        dbContext.PageData.Add(new PageDatum
        {
            LinkId = link.Id,
            SourceCode = sourceCode,
            Status = Status.LinkExtracting
        });
        link.Status = Status.SourceCodeDownloaded;
        await dbContext.SaveChangesAsync();
        await context.Publish(new LinkExtractedUrl
        {
            Id = link.Id
        });
        await context.Publish(new ClassifiedUrl
        {
            Id = link.Id
        });
        Console.WriteLine($"Link ID: {link.Id} source code downloaded.");
    }
}