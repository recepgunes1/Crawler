using Crawler.Data.Context;
using Crawler.Data.Entities;
using Crawler.Shared.Configuration;
using Crawler.Shared.Models;
using HtmlAgilityPack;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Crawler.BookParser;

public class BookParsedConsumer : IConsumer<BookParsed>
{
    public async Task Consume(ConsumeContext<BookParsed> context)
    {
        var configuration =
            new ConfigurationBuilder().AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT")}.json");
        var config = configuration.Build();
        var connectionString = config.GetConnectionString("postgresql");

        DbContextOptionsBuilder<AppDbContext> builder = new();
        builder.UseNpgsql(connectionString);

        AppDbContext dbContext = new(builder.Options);
        var link = await dbContext.Links.FindAsync(context.Message.Id) ??
                   throw new ArgumentNullException();
        var pageDatum = await dbContext.PageData.FirstAsync(p => p.LinkId == context.Message.Id) ??
                        throw new ArgumentNullException();
        var document = new HtmlDocument();
        document.LoadHtml(pageDatum.SourceCode);
        var imageLink = document.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div/meta[4]").Attributes["content"].Value;
        var title = document.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div/meta[6]").Attributes["content"]
            .Value;
        var isbn = document.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div/meta[3]").Attributes["content"]
            .Value;
        var description = document.DocumentNode.SelectSingleNode("/html/body/div[5]/div/div/meta[7]")
            .Attributes["content"].Value;
        var publisherNodes = document.DocumentNode.SelectNodes("//div[@itemprop='brand']//meta[@itemprop='name']")
            .Select(p => p.Attributes["content"].Value);
        var publisher = string.Join(";", publisherNodes);
        var book = new Book
        {
            ImageLink = imageLink,
            Title = title?.Trim(),
            Publisher = publisher.Trim(),
            Description = description?.Trim(),
            Isbn = isbn?.Trim()
        };

        link.Status = Status.Parsed;
        pageDatum.Status = Status.Parsed;
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
    }
}