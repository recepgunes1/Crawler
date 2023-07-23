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

        if (link.Url.Contains("/kitap/"))
            await context.Publish(new BookParsed
            {
                Id = link.Id
            });
        else if (link.Url.Contains("/yazar/"))
            await context.Publish(new AuthorParsed
            {
                Id = link.Id
            });
        else if (link.Url.Contains("/yayinevi/"))
            Console.WriteLine($"Link ID: {link.Id} was publisher, ain't published any service.");

        link.Status = Status.Classified;
        await dbContext.SaveChangesAsync();
        Console.WriteLine($"Link ID: {link.Id} was classified.");
    }
}