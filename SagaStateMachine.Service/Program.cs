using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachine.Service;
using SagaStateMachine.Service.Instuments;
using SagaStateMachine.Service.StateMachines;
using Shared;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<Worker>();
        services.AddMassTransit(configure =>
        {
            configure.AddSagaStateMachine<OrderStateMachine, OrderStateIntance>()
                .EntityFrameworkRepository(options =>
                {
                    options.AddDbContext<DbContext, OrderStateDbContext>((provider, builder) =>
                    {
                        builder.UseSqlServer(hostContext.Configuration.GetConnectionString("SQLServer"));
                    });
                });

            configure.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(hostContext.Configuration["RabbitMQUrl"], "/", host =>
                {
                    host.Username("guest");
                    host.Password("guest");
                });

                cfg.ReceiveEndpoint(RabbitMQSettings.StateMachine, e =>
                {
                    e.ConfigureSaga<OrderStateIntance>(provider);
                });
            }));
        });
    })
    .Build();

await host.RunAsync();