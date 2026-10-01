namespace BuildingBlocks.Messaging.RabbitMQ.Options;

public sealed class RabbitMqOptions
{
    public const string SectionName = "Messaging:RabbitMq";
    public string HostName { get; set; } = string.Empty;
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string VirtualHost { get; set; } = "/";
    public string ClientProvidedName { get; set; } = "ecommerce-api-publisher";
    public string ExchangeName { get; set; } = "ecommerce.events";
    public TimeSpan RequestedHeartbeat { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan NetworkRecoveryInterval { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan PublishTimeout { get; set; } = TimeSpan.FromSeconds(10);

}