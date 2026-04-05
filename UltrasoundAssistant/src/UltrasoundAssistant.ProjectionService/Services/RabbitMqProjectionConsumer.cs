using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UltrasoundAssistant.Contracts.Persistence.Messaging;
using UltrasoundAssistant.ProjectionService.Persistence;

namespace UltrasoundAssistant.ProjectionService.Services;

public sealed class RabbitMqProjectionConsumer(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<RabbitMqProjectionConsumer> logger) : BackgroundService
{
    private const string QueueName = "ultrasound.projection";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var host = configuration["RabbitMq:Host"] ?? "rabbitmq";
        var port = int.TryParse(configuration["RabbitMq:Port"], out var p) ? p : 5672;
        var user = configuration["RabbitMq:Username"] ?? "admin";
        var password = configuration["RabbitMq:Password"] ?? "admin";

        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = user,
            Password = password
        };

        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            DomainEventExchange.Name,
            ExchangeType.Topic,
            durable: true,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: DomainEventExchange.Name,
            routingKey: "#",
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, ea) => HandleMessageAsync(channel, ea, stoppingToken);

        await channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
    }

    private async Task HandleMessageAsync(IChannel channel, BasicDeliverEventArgs ea, CancellationToken ct)
    {
        var type = ea.BasicProperties?.Type ?? string.Empty;
        var body = Encoding.UTF8.GetString(ea.Body.Span);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ReadDbContext>();
            var processor = scope.ServiceProvider.GetRequiredService<DomainEventProcessor>();
            await processor.ProcessAsync(db, type, body, ct);
            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Projection consumer failed for {EventType}", type);
            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: ct);
        }
    }
}
