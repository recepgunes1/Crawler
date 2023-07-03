using Crawler.Data.Context;
using Crawler.Shared.Configuration;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine(builder.Configuration.GetConnectionString("postgresql"));
builder.Services.AddDbContext<AppDbContext>(p => p.UseNpgsql(builder.Configuration.GetConnectionString("postgresql")));

builder.Services.AddMassTransit(p =>
    {
        var rabbitMqConfig = builder.Configuration.GetSection("rabbitmq").Get<RabbitMqConfigModel>() ??
                             throw new ArgumentException("rabbitmq section doesnt exist");
        p.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(rabbitMqConfig.Host, rabbitMqConfig.Port, rabbitMqConfig.VirtualHost, conf =>
            {
                conf.Username(rabbitMqConfig.Username);
                conf.Password(rabbitMqConfig.Password);
            });
            cfg.ConfigureEndpoints(ctx);
        });
    }
);

builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.MapControllers();

app.Run();