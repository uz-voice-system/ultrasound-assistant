using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UltrasoundAssistant.Contracts.Events.PatientEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Common;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers;

public sealed class PatientUpdatedEventHandler : IIntegrationEventHandler
{
    private readonly ProjectionDbContext _dbContext;

    public string RoutingKey => "patient.updated";

    public PatientUpdatedEventHandler(ProjectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<PatientUpdatedEvent>(payload, JsonDefaults.Web)
                     ?? throw new InvalidOperationException("Invalid PatientUpdatedEvent payload");

        var patient = await _dbContext.Patients
            .FirstOrDefaultAsync(x => x.Id == @event.PatientId, cancellationToken);

        if (patient is null)
            return;

        if (@event.Version <= patient.Version)
            return;

        if (!string.IsNullOrWhiteSpace(@event.FullName))
            patient.FullName = @event.FullName;

        if (@event.BirthDate.HasValue)
            patient.BirthDate = @event.BirthDate.Value;

        if (@event.Gender is not null)
            patient.Gender = @event.Gender;

        patient.Version = @event.Version;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}