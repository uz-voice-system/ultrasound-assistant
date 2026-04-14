using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Messaging;

namespace UltrasoundAssistant.ProjectionService.Consumers;

public sealed class RabbitMqProjectionConsumer : BackgroundService
{
    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMqProjectionConsumer> _logger;

    private IConnection? _connection;
    private IChannel? _channel;
    private string _consumerTag = string.Empty;

    public RabbitMqProjectionConsumer(
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<RabbitMqProjectionConsumer> logger)
    {
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: _options.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        var routingKeys = new[]
        {
            "patient.created",
            "patient.updated",
            "patient.deactivated",
            "template.created",
            "template.updated",
            "template.deleted",
            "report.created",
            "report.field.updated",
            "report.completed",
            "report.deleted"
        };

        foreach (var routingKey in routingKeys)
        {
            await _channel.QueueBindAsync(
                queue: _options.Queue,
                exchange: _options.Exchange,
                routingKey: routingKey,
                arguments: null,
                cancellationToken: stoppingToken);
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var routingKey = ea.RoutingKey;
            var payload = Encoding.UTF8.GetString(ea.Body.ToArray());
            _logger.LogInformation(
                "Received message. RoutingKey={RoutingKey}, Payload={Payload}",
                routingKey,
                payload);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var handlers = scope.ServiceProvider.GetServices<IIntegrationEventHandler>();

                var matchedHandlers = handlers
                    .Where(x => string.Equals(x.RoutingKey, routingKey, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (matchedHandlers.Count == 0)
                {
                    _logger.LogWarning("No handler found for routing key {RoutingKey}", routingKey);
                    await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    return;
                }

                foreach (var handler in matchedHandlers)
                {
                    await handler.HandleAsync(payload, stoppingToken);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message with routing key {RoutingKey}", routingKey);
                await _channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        _consumerTag = await _channel.BasicConsumeAsync(
            queue: _options.Queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("Projection consumer started");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null && !string.IsNullOrWhiteSpace(_consumerTag))
        {
            await _channel.BasicCancelAsync(_consumerTag, noWait: false, cancellationToken);
        }

        if (_channel is not null)
            await _channel.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();

        await base.StopAsync(cancellationToken);
    }
}