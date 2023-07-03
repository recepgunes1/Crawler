using System.Text.RegularExpressions;
using Crawler.Data.Context;
using Crawler.Data.Entities;
using Crawler.Shared.Configuration;
using Crawler.Shared.Helper;
using Crawler.Shared.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Crawler.LinkExtractor;

public class LinkExtractedUrlConsumer : IConsumer<LinkExtractedUrl>
{
    public async Task Consume(ConsumeContext<LinkExtractedUrl> context)
    {
        const string urlPattern = @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?";
        var matches = Regex.Matches(context.Message.SourceCode, urlPattern)
            .Select(p => p.Value)
            .Distinct();

        var configuration = new ConfigurationBuilder().AddJsonFile($"appsettings.Development.json");
        var config = configuration.Build();
        var connectionString = config.GetConnectionString("postgresql");

        DbContextOptionsBuilder<AppDbContext> builder = new();
        builder.UseNpgsql(connectionString);

        AppDbContext dbContext = new(builder.Options);
        List<Link> linksToInsert = new List<Link>();

        foreach (var match in matches)
        {
            if (await dbContext.Links.AnyAsync(p => p.Url == match))
            {
                continue;
            }
            var flagMatchingHost = Url.GetRegistrableDomain(match) == context.Message.Host &&
                                   Url.GetSubDomain(match) == "www";
            var link = new Link
            {
                SourceId = context.Message.LinkId,
                Url = match,
                Status = flagMatchingHost ? Status.Requested : Status.DontCrawl
            };

            linksToInsert.Add(link);

            if (flagMatchingHost)
            {
                await context.Publish(new RequestedUrl
                    { Id = link.Id, Url = match });
            }
        }

        // Insert all links at once
        if(linksToInsert.Count > 0)
        {
            await dbContext.BulkInsertAsync(linksToInsert);
        }

        var page = await dbContext.PageData.FirstAsync(p => p.LinkId == context.Message.LinkId);
        page.Status = Status.LinkExtracted;

        try
        {
            await dbContext.BulkSaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("=======================================================");
            Console.WriteLine($"{ex.Message}");
            Console.WriteLine("=======================================================");
            throw;
        }
    }
}
