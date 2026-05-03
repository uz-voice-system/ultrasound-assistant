using UltrasoundAssistant.Contracts.Events.PatientEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers.Patients;

public sealed class PatientDeletedEventHandler : IIntegrationEventHandler
{
    private readonly IPatientReadRepository _repository;

    public string RoutingKey => "patient.deleted";

    public PatientDeletedEventHandler(IPatientReadRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = EventPayloadReader.Read<PatientDeletedEvent>(payload, nameof(PatientDeletedEvent));

        var patient = await _repository.GetByIdForUpdateAsync(@event.PatientId, cancellationToken);

        if (patient is null)
            return;

        if (@event.Version <= patient.Version)
            return;

        patient.IsDeleted = true;
        patient.Version = @event.Version;

        await _repository.SaveChangesAsync(cancellationToken);
    }
}
