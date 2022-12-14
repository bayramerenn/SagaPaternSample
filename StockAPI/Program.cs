using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using StockAPI.Consumers;
using StockAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
               options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreConnection")));

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<OrderCreateEventConsumer>();
    configure.AddConsumer<PaymentFailedEventConsumer>();

    configure.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQUrl"], "/", host =>
        {
            host.Username("guest");
            host.Password("guest");
        });

        cfg.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e =>
        {
            e.ConfigureConsumer<OrderCreateEventConsumer>(context);
        });

        cfg.ReceiveEndpoint(RabbitMQSettings.Stock_PaymentFailedEventQueue, e =>
        {
            e.ConfigureConsumer<PaymentFailedEventConsumer>(context);
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