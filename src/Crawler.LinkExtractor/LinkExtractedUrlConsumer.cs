using System.Text.RegularExpressions;
using Crawler.Data.Context;
using Crawler.Data.Entities;
using Crawler.Shared.Configuration;
using Crawler.Shared.Models;
using MassTransit;

namespace Crawler.LinkExtractor;

public class LinkExtractedUrlConsumer : IConsumer<LinkExtractedUrl>
{
    public async Task Consume(ConsumeContext<LinkExtractedUrl> context)
    {
        try
        {
            Console.WriteLine($"LinkExtractedUrl.Url is {context.Message.Url} in LinkExtractedUrlConsumer");
            const string urlPattern = @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?";
            var matches = Regex.Matches(context.Message.SourceCode, urlPattern);

            await using var dbContext = new AppDbContext();
            var source = await dbContext.Links.FindAsync(context.Message.Id);

            if (source == null)
            {
                Console.WriteLine($"No link found for {context.Message.Url}");
                return;
            }

            source.Status = Status.LinkExtracting;
            await dbContext.SaveChangesAsync();

            foreach (var match in matches.Select(p => p.Value).Distinct())
            {
                if (dbContext.Links.Any(p => p.Url == match)) continue;

                var link = new Link { Url = match, SourceId = source.Id, Status = Status.Requested };

                var hostParent = Helper.GetRegistrableDomain(context.Message.Url);
                var hostChild = Helper.GetRegistrableDomain(match);

                if (hostChild == hostParent)
                {
                    await context.Publish(new RequestedUrl { Id = Guid.NewGuid().ToString(), Url = match });
                }
                else
                {
                    link.Status = Status.DontCrawl;
                }

                await dbContext.Links.AddAsync(link);
            }

            source.Status = Status.LinkExtracted;
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw; // re-throw the exception if you want it to be handled by upper layers
        }
    }
}