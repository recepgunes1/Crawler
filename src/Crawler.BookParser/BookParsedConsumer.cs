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
        var imageLink = document.DocumentNode
            .SelectSingleNode("/html/body/div[5]/div/div/div[7]/div/div[1]/div[1]/div/div/div/div[1]/a/img")
            .Attributes["src"].Value;
        var title = document.DocumentNode
            .SelectSingleNode("/html/body/div[5]/div/div/div[7]/div/div[2]/div[1]")
            .InnerText;
        var author = document.DocumentNode
            .SelectSingleNode("/html/body/div[5]/div/div/div[7]/div/div[2]/div[2]/div[1]/div[1]/div[1]/div[1]")
            .InnerText;
        var publisher = document.DocumentNode
            .SelectSingleNode("/html/body/div[5]/div/div/div[7]/div/div[2]/div[2]/div[1]/div[1]/div[1]/div[3]")
            .InnerText;
        var description = document.DocumentNode
            .SelectSingleNode("/html/body/div[5]/div/div/div[7]/div/div[2]/div[2]/div[1]/div[1]/div[3]")
            .InnerText;
        // var category = document.DocumentNode
        //     .SelectSingleNode("/html/body/div[5]/div/div/div[7]/div/div[2]/div[2]/div[1]/div[2]/div[2]/div/ul").InnerText;
        var book = new Book
        {
            ImageLink = imageLink,
            Title = title.Trim(),
            Author = author.Trim(),
            Publisher = publisher.Trim(),
            Description = description.Trim()
            // Category = category.Trim()
        };

        link.Status = Status.Parsed;
        pageDatum.Status = Status.Parsed;
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
    }
}