using System;
using BuildingBlocks.Messaging.Abstractions.Publishing;
using BuildingBlocks.Messaging.RabbitMQ.Connections;
using BuildingBlocks.Messaging.RabbitMQ.Options;
using BuildingBlocks.Messaging.RabbitMQ.Publishing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging.RabbitMQ.DependencyInjection;

public static class RabbitMqServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<RabbitMqOptions>()
            .Bind(
                configuration.GetSection(
                    RabbitMqOptions.SectionName))
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.HostName),
                "RabbitMQ host name is required.")
            .Validate(
                options =>
                    options.Port is > 0 and <= 65535,
                "RabbitMQ port must be between 1 and 65535.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.UserName),
                "RabbitMQ user name is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.Password),
                "RabbitMQ password is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.VirtualHost),
                "RabbitMQ virtual host is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.ClientProvidedName),
                "RabbitMQ client-provided name is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.ExchangeName),
                "RabbitMQ exchange name is required.")
            .Validate(
                options =>
                    options.RequestedHeartbeat >
                    TimeSpan.Zero,
                "RabbitMQ heartbeat must be greater than zero.")
            .Validate(
                options =>
                    options.NetworkRecoveryInterval >
                    TimeSpan.Zero,
                "RabbitMQ network recovery interval must be greater than zero.")
            .Validate(
                options =>
                    options.ConnectionTimeout >
                    TimeSpan.Zero,
                "RabbitMQ connection timeout must be greater than zero.")
            .Validate(
                options =>
                    options.PublishTimeout >
                    TimeSpan.Zero,
                "RabbitMQ publish timeout must be greater than zero.")
            .ValidateOnStart();

        services.AddSingleton<
            IRabbitMqConnectionProvider,
            RabbitMqConnectionProvider>();

        services.AddSingleton<
            IIntegrationEventPublisher,
            RabbitMqIntegrationEventPublisher>();

        return services;
    }
}
