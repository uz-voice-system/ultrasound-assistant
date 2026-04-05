namespace UltrasoundAssistant.Contracts.Commands.Common;

public sealed class CommandEnvelope<T>
{
    public Guid CommandId { get; set; }
    public T Command { get; set; } = default!;
}
