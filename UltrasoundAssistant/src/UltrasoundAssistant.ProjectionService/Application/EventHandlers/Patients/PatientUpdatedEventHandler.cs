using UltrasoundAssistant.Contracts.Entity.Patients;
using UltrasoundAssistant.Contracts.Events.PatientEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Abstractions;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Patients;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers.Patients;

public sealed class PatientUpdatedEventHandler : IIntegrationEventHandler
{
    private readonly IPatientReadRepository _repository;

    public string RoutingKey => "patient.updated";

    public PatientUpdatedEventHandler(IPatientReadRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = EventPayloadReader.Read<PatientUpdatedEvent>(payload, nameof(PatientUpdatedEvent));

        var patient = await _repository.GetByIdForUpdateAsync(@event.PatientId, cancellationToken);

        if (patient is null)
            return;

        if (@event.Version <= patient.Version)
            return;

        patient.FullName = @event.FullName;
        patient.BirthDate = @event.BirthDate;
        patient.Gender = @event.Gender;
        patient.PhoneNumber = @event.PhoneNumber;
        patient.Email = @event.Email;
        patient.IsDeleted = false;
        patient.Version = @event.Version;

        SyncDocuments(patient, @event.Documents);

        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static void SyncDocuments(PatientReadModel patient, IReadOnlyList<PatientDocumentDto> documents)
    {
        var incomingIds = documents
            .Select(x => x.Id)
            .ToHashSet();

        var documentsToRemove = patient.Documents
            .Where(x => !incomingIds.Contains(x.Id))
            .ToList();

        foreach (var document in documentsToRemove)
            patient.Documents.Remove(document);

        foreach (var documentEvent in documents)
        {
            var document = patient.Documents
                .FirstOrDefault(x => x.Id == documentEvent.Id);

            if (document is null)
            {
                document = new PatientDocumentReadModel
                {
                    Id = documentEvent.Id,
                    PatientId = patient.Id
                };

                patient.Documents.Add(document);
            }

            document.DocumentType = documentEvent.DocumentType;
            document.Series = documentEvent.Series;
            document.Number = documentEvent.Number;
            document.IssuedBy = documentEvent.IssuedBy;
            document.IssueDate = documentEvent.IssueDate;
            document.DepartmentCode = documentEvent.DepartmentCode;
            document.Organization = documentEvent.Organization;
        }
    }
}
