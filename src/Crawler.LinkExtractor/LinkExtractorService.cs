using Crawler.Shared.Configuration;
using MassTransit;
using Topshelf;

namespace Crawler.LinkExtractor;

public class LinkExtractorService
{
    private readonly IBusControl _busControl;

    public LinkExtractorService(string queueName)
    {
        _busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host(RabbitMqCredentials.Host, RabbitMqCredentials.Port, RabbitMqCredentials.VirtualHost, s =>
            {
                s.Username(RabbitMqCredentials.Username);
                s.Password(RabbitMqCredentials.Password);
            });            
            cfg.ReceiveEndpoint(queueName, e => e.Consumer<LinkExtractedUrlConsumer>());
        });
    }

    public bool Start(HostControl? hostControl)
    {
        _busControl.StartAsync().GetAwaiter().GetResult();
        return true;
    }

    public bool Stop(HostControl? hostControl)
    {
        _busControl.StopAsync().GetAwaiter().GetResult();
        return true;
    }
}