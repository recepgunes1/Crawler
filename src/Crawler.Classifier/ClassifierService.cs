using Crawler.Shared.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Topshelf;

namespace Crawler.Classifier;

public class ClassifierService
{
    private readonly IBusControl _busControl;

    public ClassifierService(string queueName, IConfiguration configuration)
    {
        var rabbitMqConfig = configuration.GetSection("rabbitmq").Get<RabbitMqConfigModel>() ??
                             throw new ArgumentException("rabbitmq section doesnt exist");
        _busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            cfg.Host(rabbitMqConfig.Host, rabbitMqConfig.Port, rabbitMqConfig.VirtualHost, s =>
            {
                s.Username(rabbitMqConfig.Username);
                s.Password(rabbitMqConfig.Password);
            });
            cfg.ReceiveEndpoint(queueName, e =>
            {
                e.Consumer<ClassifiedUrlConsumer>();
                e.Consumer<ClassifiedUrlFaultConsumer>();
            });
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