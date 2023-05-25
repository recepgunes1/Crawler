using Crawler.Data.Context;
using Crawler.Data.Entities;
using Crawler.Shared.Configuration;
using Crawler.Shared.Models;
using MassTransit;

namespace Crawler.PageDownloader;

public class PageDownloadedConsumer : IConsumer<RequestedUrl>
{
    public async Task Consume(ConsumeContext<RequestedUrl> context)
    {
        Console.WriteLine($"RequestedUrl.Id is {context.Message.Url} in PageDownloadedConsumer");

        using HttpClient client = new();

        var sourceCode = await client.GetStringAsync(context.Message.Url);
        
        await using AppDbContext dbContext = new();
        
        dbContext.PageData.Add(new PageDatum
        {
            SourceCode = sourceCode,
            Status = Status.SourceCodeDownloaded,
            LinkId = context.Message.Id
        });

        var url = dbContext.Links.FirstOrDefault(p => p.Url == context.Message.Url);
        if (url != null)
        {
            url.Status = Status.SourceCodeDownloaded;
            await dbContext.SaveChangesAsync();
        }
        else
        {
            Console.WriteLine($"No matching URL found for {context.Message.Url}");
        }

        await context.Publish(new LinkExtractedUrl
        {
            Id = context.Message.Id,
            Url = context.Message.Url,
            SourceCode = sourceCode
        });
    }
}