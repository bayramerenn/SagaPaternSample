using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Consumer;
using OrderAPI.Modes.Context;
using Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<OrderCompletedEventConsumer>();
    configure.AddConsumer<OrderFailedEventConsumer>();

    configure.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQUrl"], "/", host =>
        {
            host.Username("guest");
            host.Password("guest");
        });

        cfg.ReceiveEndpoint(RabbitMQSettings.Order_OrderCompletedEventQueue, e =>
        {
            e.ConfigureConsumer<OrderCompletedEventConsumer>(context);
        });

        cfg.ReceiveEndpoint(RabbitMQSettings.Order_OrderFailedEventQueue, e =>
        {
            e.ConfigureConsumer<OrderFailedEventConsumer>(context);
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();