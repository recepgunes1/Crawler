using Crawler.Data.Context;
using Crawler.Data.Entities;
using Crawler.Shared.Configuration;
using Crawler.Shared.Models;
using HtmlAgilityPack;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Crawler.AuthorParser;

public class AuthorParsedConsumer : IConsumer<AuthorParsed>
{
    public async Task Consume(ConsumeContext<AuthorParsed> context)
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

        //parse author value 

        var author = new Author();
        link.Status = Status.Parsed;
        pageDatum.Status = Status.Parsed;
        dbContext.Authors.Add(author);
        await dbContext.SaveChangesAsync();
    }
}