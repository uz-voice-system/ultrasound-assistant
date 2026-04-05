using System.Text;
using RabbitMQ.Client;

namespace UltrasoundAssistant.AggregationService.Services;

public sealed class RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger) : IMessageBrokerPublisher
{
    private readonly string _host = configuration["RabbitMq:Host"] ?? "rabbitmq";
    private readonly int _port = int.TryParse(configuration["RabbitMq:Port"], out var port) ? port : 5672;
    private readonly string _user = configuration["RabbitMq:Username"] ?? "admin";
    private readonly string _password = configuration["RabbitMq:Password"] ?? "admin";

    public async Task PublishAsync(string exchange, string routingKey, string eventType, string payload, CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _host,
            Port = _port,
            UserName = _user,
            Password = _password
        };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true, cancellationToken: cancellationToken);

        var props = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Type = eventType,
            MessageId = Guid.NewGuid().ToString("N"),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        var body = Encoding.UTF8.GetBytes(payload);
        await channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);

        logger.LogInformation("Published {EventType} to {Exchange}:{RoutingKey}", eventType, exchange, routingKey);
    }
}
