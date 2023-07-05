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
        var configuration =
            new ConfigurationBuilder().AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT")}.json");
        var config = configuration.Build();
        var connectionString = config.GetConnectionString("postgresql");

        DbContextOptionsBuilder<AppDbContext> builder = new();
        builder.UseNpgsql(connectionString);

        AppDbContext dbContext = new(builder.Options);

        var target = (await dbContext.Links.FindAsync(context.Message.Id))!;
        using HttpClient client = new();

        client.DefaultRequestHeaders.Add("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.150 Safari/537.36");
        client.DefaultRequestHeaders.Add("Referer", target.Url);
        string sourceCode;
        try
        {
            sourceCode = await client.GetStringAsync(target.Url);
        }
        catch (Exception ex)
        {
            Console.WriteLine("=======================================================");
            Console.WriteLine(ex.ToJsonString());
            Console.WriteLine("=======================================================");
            throw;
        }


        dbContext.PageData.Add(new PageDatum
        {
            LinkId = context.Message.Id,
            SourceCode = sourceCode,
            Status = Status.LinkExtracting
        });
        await dbContext.SaveChangesAsync();
        await context.Publish(new LinkExtractedUrl
        {
            Id = context.Message.Id
        });
        Console.WriteLine($"Page Downloaded Url ID: {context.Message.Id}");
    }
}