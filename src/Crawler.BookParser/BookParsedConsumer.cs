using System.Text;
using Crawler.BookParser.JsonModels;
using Crawler.Data.Context;
using Crawler.Data.Entities;
using Crawler.Shared.Configuration;
using Crawler.Shared.Models;
using HtmlAgilityPack;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

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
        var jsonData = document.DocumentNode.SelectSingleNode("/html/body/div[5]/div/script[1]").InnerText;
        var bytes = Encoding.Default.GetBytes(jsonData);
        var jsonString = Encoding.UTF8.GetString(bytes);
        var jsonBook = JsonConvert.DeserializeObject<JsonBook>(jsonString);
        var book = new Book
        {
            LinkId = link.Id,
            ImageLink = jsonBook?.Image.Trim(),
            Title = jsonBook?.Name.Trim(),
            Isbn = jsonBook?.Isbn.Trim(),
            Pages = jsonBook?.NumberOfPages.Trim(),
            Author = jsonBook?.Author.Name.Trim(),
            Publisher = jsonBook?.Publisher.Name.Trim(),
            Description = jsonBook?.Description.Trim()
        };
        link.Status = Status.Parsed;
        pageDatum.Status = Status.Parsed;
        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();
    }
}