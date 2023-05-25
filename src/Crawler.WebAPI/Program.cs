using Crawler.Data.Context;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>();

builder.Services.AddMassTransit(p =>
    {
        p.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host("s_rabbitmq", 5672, "/", conf =>
            {
                conf.Username("guest");
                conf.Password("guest");
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

using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope())
{
    var context = serviceScope?.ServiceProvider.GetRequiredService<AppDbContext>();
    context?.Database.Migrate();
}

app.MapControllers();

app.Run();