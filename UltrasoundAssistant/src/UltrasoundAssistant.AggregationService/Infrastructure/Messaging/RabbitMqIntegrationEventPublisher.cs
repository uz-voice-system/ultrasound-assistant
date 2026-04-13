using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using UltrasoundAssistant.AggregationService.Application.Abstractions;
using UltrasoundAssistant.AggregationService.Infrastructure.Persistence;

namespace UltrasoundAssistant.AggregationService.Infrastructure.Messaging;

public sealed class RabbitMqIntegrationEventPublisher : IIntegrationEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqIntegrationEventPublisher> _logger;

    private IConnection? _connection;
    private IChannel? _channel;
    private bool _initialized;

    public RabbitMqIntegrationEventPublisher(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqIntegrationEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync(
        string aggregateType,
        Guid aggregateId,
        IReadOnlyList<EventRecord> events,
        CancellationToken cancellationToken)
    {
        await EnsureInitializedAsync(cancellationToken);

        foreach (var @event in events)
        {
            var body = Encoding.UTF8.GetBytes(@event.Payload);

            var properties = new BasicProperties
            {
                Persistent = true,
                MessageId = @event.EventId.ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Type = @event.EventType,
                ContentType = "application/json"
            };

            await _channel!.BasicPublishAsync(
                exchange: _options.Exchange,
                routingKey: @event.RoutingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Published integration event: AggregateType={AggregateType}, AggregateId={AggregateId}, EventType={EventType}, Version={Version}, RoutingKey={RoutingKey}",
                aggregateType,
                aggregateId,
                @event.EventType,
                @event.Version,
                @event.RoutingKey);
        }
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
            return;

        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        _initialized = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}