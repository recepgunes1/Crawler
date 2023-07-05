using Crawler.Shared.Helper;
using Crawler.Shared.Models;
using MassTransit;

namespace Crawler.PageDownloader;

public class PageDownloadedFaultConsumer : IConsumer<Fault<RequestedUrl>>
{
    public async Task Consume(ConsumeContext<Fault<RequestedUrl>> context)
    {
        foreach (var exception in context.Message.Exceptions)
            await Console.Out.WriteLineAsync(exception.ToJsonString());
    }
}