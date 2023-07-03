using Crawler.Data.Context;
using Crawler.Data.Entities;
using Crawler.Shared.Configuration;
using Crawler.Shared.Helper;
using Crawler.Shared.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Crawler.PageDownloader;

public class PageDownloadedConsumer : IConsumer<RequestedUrl>
{
    public async Task Consume(ConsumeContext<RequestedUrl> context)
    {
        using HttpClient client = new();

        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.150 Safari/537.36");
        client.DefaultRequestHeaders.Add("Referer", context.Message.Url);
        string sourceCode = string.Empty;
        try
        {
            sourceCode = await client.GetStringAsync(context.Message.Url);
        }
        catch (Exception ex)
        {
            Console.WriteLine("=======================================================");
            Console.WriteLine($"{context.Message.Url}----{ex.Message}");
            Console.WriteLine("=======================================================");
            throw;
        }
        
        
        var configuration =  new ConfigurationBuilder().AddJsonFile($"appsettings.Development.json");
        var config = configuration.Build();
        var connectionString = config.GetConnectionString("postgresql");
        
        DbContextOptionsBuilder<AppDbContext> builder = new();
        builder.UseNpgsql(connectionString);
        
        AppDbContext dbContext = new(builder.Options);
        dbContext.PageData.Add(new PageDatum()
        {
            LinkId = context.Message.Id,
            SourceCode = sourceCode,
            Status = Status.LinkExtracting
        });
        await dbContext.SaveChangesAsync();
        await context.Publish(new LinkExtractedUrl
        {
            LinkId = context.Message.Id, SourceCode = sourceCode,
            Host = Url.GetRegistrableDomain(context.Message.Url)
        });
        Console.WriteLine($"consumed {context.Message.Url}");
    }
}