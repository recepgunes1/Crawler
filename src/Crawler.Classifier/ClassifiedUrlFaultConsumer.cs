using Crawler.Shared.Helper;
using Crawler.Shared.Models;
using MassTransit;

namespace Crawler.Classifier;

public class ClassifiedUrlFaultConsumer : IConsumer<Fault<ClassifiedUrl>>
{
    public async Task Consume(ConsumeContext<Fault<ClassifiedUrl>> context)
    {
        foreach (var exception in context.Message.Exceptions)
            await Console.Out.WriteLineAsync(exception.ToJsonString());
    }
}