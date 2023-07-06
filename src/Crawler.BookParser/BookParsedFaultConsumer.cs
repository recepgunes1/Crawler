using Crawler.Shared.Helper;
using Crawler.Shared.Models;
using MassTransit;

namespace Crawler.BookParser;

public class BookParsedFaultConsumer : IConsumer<Fault<BookParsed>>
{
    public async Task Consume(ConsumeContext<Fault<BookParsed>> context)
    {
        foreach (var exception in context.Message.Exceptions)
            await Console.Out.WriteLineAsync(exception.ToJsonString());
    }
}