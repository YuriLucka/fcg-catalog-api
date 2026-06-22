using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fcg.Messaging;

public static class MassTransitExtensions
{
    // Lê RabbitMq:Host/User/Pass (ConfigMap + Secret). Registra retry padrão.
    public static IServiceCollection AddFcgMessaging(
        this IServiceCollection services,
        IConfiguration cfg,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            configureConsumers?.Invoke(x);

            x.UsingRabbitMq((ctx, bus) =>
            {
                bus.Host(cfg["RabbitMq:Host"] ?? "rabbitmq", h =>
                {
                    h.Username(cfg["RabbitMq:User"] ?? "guest");
                    h.Password(cfg["RabbitMq:Pass"] ?? "guest");
                });
                bus.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                bus.ConfigureEndpoints(ctx);
            });
        });
        return services;
    }
}
