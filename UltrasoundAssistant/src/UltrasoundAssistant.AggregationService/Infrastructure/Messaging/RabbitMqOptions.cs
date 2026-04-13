namespace UltrasoundAssistant.AggregationService.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Exchange { get; set; } = "ultrasound.events";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
}