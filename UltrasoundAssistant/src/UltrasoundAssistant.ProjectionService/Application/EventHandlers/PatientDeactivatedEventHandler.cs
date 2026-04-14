using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UltrasoundAssistant.Contracts.Events.PatientEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Common;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers;

public sealed class PatientDeactivatedEventHandler : IIntegrationEventHandler
{
    private readonly ProjectionDbContext _dbContext;

    public string RoutingKey => "patient.deactivated";

    public PatientDeactivatedEventHandler(ProjectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<PatientDeactivatedEvent>(payload, JsonDefaults.Web)
                     ?? throw new InvalidOperationException("Invalid PatientDeactivatedEvent payload");

        var patient = await _dbContext.Patients
            .FirstOrDefaultAsync(x => x.Id == @event.PatientId, cancellationToken);

        if (patient is null)
            return;

        if (@event.Version <= patient.Version)
            return;

        patient.IsDeleted = @event.IsDeleted;
        patient.Version = @event.Version;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}