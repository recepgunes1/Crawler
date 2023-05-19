using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(p =>
    p.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("s_rabbitmq", 5672, "/", conf =>
        {
            conf.Username("guest");
            conf.Password("guest");
        });
    })
);

builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.MapControllers();

app.Run();
