namespace UltrasoundAssistant.ProjectionService.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public string Host { get; set; } = "rabbitmq";

    public int Port { get; set; } = 5672;

    public string Exchange { get; set; } = "ultrasound.events";

    public string Queue { get; set; } = "projection-service";

    public string Username { get; set; } = "admin";

    public string Password { get; set; } = "admin";

    public string VirtualHost { get; set; } = "/";

    public string DeadLetterExchange { get; set; } = "ultrasound.events.dlx";

    public string DeadLetterQueue { get; set; } = "projection-service.dead";

    public string DeadLetterRoutingKey { get; set; } = "projection.dead";
}
