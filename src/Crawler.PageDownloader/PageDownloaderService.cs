using Crawler.Shared.Configuration;
using MassTransit;
using Topshelf;

namespace Crawler.PageDownloader;

public class PageDownloaderService
{
    private readonly IBusControl _busControl;

    public PageDownloaderService(string queueName)
    {
        _busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host(RabbitMqCredentials.Host, RabbitMqCredentials.Port, RabbitMqCredentials.VirtualHost, s =>
            {
                s.Username(RabbitMqCredentials.Username);
                s.Password(RabbitMqCredentials.Password);
            });
            cfg.ReceiveEndpoint(queueName, e => e.Consumer<PageDownloadedConsumer>());
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