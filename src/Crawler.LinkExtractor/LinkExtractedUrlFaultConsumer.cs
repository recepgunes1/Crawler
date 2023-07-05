using Crawler.Shared.Helper;
using Crawler.Shared.Models;
using MassTransit;

namespace Crawler.LinkExtractor;

public class LinkExtractedUrlFaultConsumer : IConsumer<Fault<LinkExtractedUrl>>
{
    public async Task Consume(ConsumeContext<Fault<LinkExtractedUrl>> context)
    {
        foreach (var exception in context.Message.Exceptions)
            await Console.Out.WriteLineAsync(exception.ToJsonString());
    }
}