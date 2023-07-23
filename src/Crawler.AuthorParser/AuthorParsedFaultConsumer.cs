using Crawler.Shared.Helper;
using Crawler.Shared.Models;
using MassTransit;

namespace Crawler.AuthorParser;

public class AuthorParsedFaultConsumer : IConsumer<Fault<AuthorParsed>>
{
    public async Task Consume(ConsumeContext<Fault<AuthorParsed>> context)
    {
        foreach (var exception in context.Message.Exceptions)
            await Console.Out.WriteLineAsync(exception.ToJsonString());
    }
}