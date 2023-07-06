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
        //image xpath:  /html/body/div[5]/div/div/div[8]/div/div[1]/div[1]/div/div[1]/div/div[1]/a
        //book title:   /html/body/div[5]/div/div/div[8]/div/div[2]/div[1]/h1
        //author:       /html/body/div[5]/div/div/div[8]/div/div[2]/div[2]/div[1]/div[1]/div[1]/div[1]/div/a
        //publisher:    /html/body/div[5]/div/div/div[8]/div/div[2]/div[2]/div[1]/div[1]/div[1]/div[3]/div/a
        //description   /html/body/div[5]/div/div/div[8]/div/div[2]/div[2]/div[1]/div[1]/div[3]/div
        //isbn:         /html/body/div[5]/div/div/div[8]/div/div[2]/div[2]/div[1]/div[2]/div[1]/div[1]/table/tbody/tr[3]/td[2]
        //pages:        /html/body/div[5]/div/div/div[8]/div/div[2]/div[2]/div[1]/div[2]/div[1]/div[1]/table/tbody/tr[5]/td[2]
        //category      /html/body/div[5]/div/div/div[8]/div/div[2]/div[2]/div[1]/div[2]/div[3]/div
        var imageLink = document.DocumentNode
            .SelectSingleNode("/html/body/div[5]/div/div/div[8]/div/div[1]/div[1]/div/div[1]/div/div[1]/a")
            .Attributes["href"].Value;
        var title = document.DocumentNode
            .SelectSingleNode("/html/body/div[5]/div/div/div[8]/div/div[2]/div[1]/h1")
            .InnerText;
        var author = document.DocumentNode
            .SelectSingleNode("/html/body/div[5]/div/div/div[8]/div/div[2]/div[2]/div[1]/div[1]/div[1]/div[1]/div/a")
            .InnerText;
        var publisher = document.DocumentNode
            .SelectSingleNode("/html/body/div[5]/div/div/div[8]/div/div[2]/div[2]/div[1]/div[1]/div[1]/div[3]/div/a")
            .InnerText;
        var description = document.DocumentNode
            .SelectSingleNode("/html/body/div[5]/div/div/div[8]/div/div[2]/div[2]/div[1]/div[1]/div[3]/div")
            .InnerText;
        var isbn = document.DocumentNode
            .SelectSingleNode(
                "/html/body/div[5]/div/div/div[8]/div/div[2]/div[2]/div[1]/div[2]/div[1]/div[1]/table/tbody/tr[3]/td[2]")
            .InnerText;
        var pages = document.DocumentNode
            .SelectSingleNode(
                "/html/body/div[5]/div/div/div[8]/div/div[2]/div[2]/div[1]/div[2]/div[1]/div[1]/table/tbody/tr[5]/td[2]")
            .InnerText;
        var category = document.DocumentNode
            .SelectSingleNode("/html/body/div[5]/div/div/div[8]/div/div[2]/div[2]/div[1]/div[2]/div[3]/div]").InnerText;

        var book = new Book
        {
            ImageLink = imageLink,
            Title = title,
            Author = author,
            Publisher = publisher,
            Description = description,
            Isbn = isbn,
            Pages = pages,
            Category = category
        };

        link.Status = Status.Parsed;
        pageDatum.Status = Status.Parsed;
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
    }
}