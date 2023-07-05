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
        var configuration =
            new ConfigurationBuilder().AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT")}.json");
        var config = configuration.Build();
        var connectionString = config.GetConnectionString("postgresql");

        DbContextOptionsBuilder<AppDbContext> builder = new();
        builder.UseNpgsql(connectionString);

        AppDbContext dbContext = new(builder.Options);
        var linksToInsert = new List<Link>();

        var pageDatum = await dbContext.PageData.FirstAsync(p => p.LinkId == context.Message.Id);
        var target = await dbContext.Links.FindAsync(context.Message.Id);
        const string urlPattern = @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?";
        var matches = Regex.Matches(pageDatum.SourceCode, urlPattern)
            .Select(p => p.Value)
            .Distinct();


        foreach (var match in matches)
        {
            if (dbContext.Links.Any(p => p.Url == match)) continue;
            var flagMatchingHost =
                UrlHelpers.GetRegistrableDomain(match) == UrlHelpers.GetRegistrableDomain(target.Url) &&
                UrlHelpers.GetSubDomain(match) == "www" &&
                UrlHelpers.IsValidUrl(match);
            var link = new Link
            {
                SourceId = pageDatum.LinkId,
                Url = match,
                Status = flagMatchingHost ? Status.Requested : Status.DontCrawl
            };

            linksToInsert.Add(link);

            if (flagMatchingHost)
                await context.Publish(new RequestedUrl
                    { Id = link.Id });
        }

        // Insert all links at once
        if (linksToInsert.Count > 0) await dbContext.BulkInsertAsync(linksToInsert);

        var page = await dbContext.PageData.FirstAsync(p => p.LinkId == pageDatum.LinkId);
        page.Status = Status.LinkExtracted;
        var url = await dbContext.Links.FindAsync(pageDatum.LinkId);
        url!.Status = Status.SourceCodeDownloaded;
        try
        {
            await dbContext.BulkSaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("=======================================================");
            Console.WriteLine(ex.ToJsonString());
            Console.WriteLine("=======================================================");
            throw;
        }
    }
}