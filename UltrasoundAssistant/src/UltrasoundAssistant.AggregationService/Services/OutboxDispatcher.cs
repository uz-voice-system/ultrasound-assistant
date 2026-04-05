using UltrasoundAssistant.AggregationService.Infrastructure;

namespace UltrasoundAssistant.AggregationService.Services;

public sealed class OutboxDispatcher(IServiceScopeFactory scopeFactory, ILogger<OutboxDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var store = scope.ServiceProvider.GetRequiredService<IEventStore>();
                var publisher = scope.ServiceProvider.GetRequiredService<IMessageBrokerPublisher>();
                var batch = await store.ReadPendingOutboxAsync(batchSize: 50, stoppingToken);

                foreach (var message in batch)
                {
                    try
                    {
                        await publisher.PublishAsync(message.Exchange, message.RoutingKey, message.EventType, message.Payload, stoppingToken);
                        await store.MarkOutboxPublishedAsync(message.Id, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to publish outbox message {OutboxId}", message.Id);
                        await store.MarkOutboxFailedAsync(message.Id, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox dispatcher failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
