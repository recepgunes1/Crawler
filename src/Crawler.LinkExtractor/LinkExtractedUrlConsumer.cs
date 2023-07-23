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
        var pageDatum = dbContext.PageData.First(p => p.LinkId == context.Message.Id) ??
                        throw new ArgumentNullException();
        var link = await dbContext.Links.FindAsync(context.Message.Id) ?? throw new ArgumentNullException();
        const string urlPattern = @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?";
        var matches = Regex.Matches(pageDatum.SourceCode, urlPattern)
            .Select(p => p.Value)
            .Distinct();
        foreach (var match in matches)
        {
            if (dbContext.Links.Any(p => p.Url == match)) continue;
            var flagMatchingHost =
                UrlHelpers.GetRegistrableDomain(match) == UrlHelpers.GetRegistrableDomain(link.Url) &&
                UrlHelpers.GetSubDomain(match) == "www";

            var newLink = new Link
            {
                SourceId = pageDatum.LinkId,
                Url = match,
                Status = flagMatchingHost ? Status.Requested : Status.DontCrawl
            };

            dbContext.Links.Add(newLink);
            await dbContext.SaveChangesAsync();
            if (flagMatchingHost)
                await context.Publish(new RequestedUrl
                    { Id = newLink.Id });
        }

        pageDatum.Status = Status.LinkExtracted;
        link.Status = Status.LinkExtracted;
        await dbContext.SaveChangesAsync();
        Console.WriteLine($"Link ID: {link.Id} links extracted.");
    }
}