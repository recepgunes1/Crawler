using Crawler.Data.Context;
using Crawler.Shared.Configuration;
using Crawler.Shared.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Crawler.Classifier;

public class ClassifiedUrlConsumer : IConsumer<ClassifiedUrl>
{
    public async Task Consume(ConsumeContext<ClassifiedUrl> context)
    {
        var configuration =
            new ConfigurationBuilder().AddJsonFile(
                $"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT")}.json");
        var config = configuration.Build();
        var connectionString = config.GetConnectionString("postgresql");

        DbContextOptionsBuilder<AppDbContext> builder = new();
        builder.UseNpgsql(connectionString);

        AppDbContext dbContext = new(builder.Options);
        var link = await dbContext.Links.FindAsync(context.Message.Id) ?? throw new ArgumentNullException();
        link.Status = Status.Classified;
        if (link.Url.Contains("/kitap/"))
            await context.Publish(new BookParsed
            {
                Id = link.Id
            });
        else if (link.Url.Contains("/yazar/"))
            Console.WriteLine($"{link.Url}-->yazar");
        else if (link.Url.Contains("/yayinevi/"))
            Console.WriteLine($"{link.Url}-->yayinevi");
        await dbContext.SaveChangesAsync();
    }
}