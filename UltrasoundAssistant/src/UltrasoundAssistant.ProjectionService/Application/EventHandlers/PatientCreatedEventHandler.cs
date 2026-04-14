using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UltrasoundAssistant.Contracts.Events.PatientEvent;
using UltrasoundAssistant.ProjectionService.Application.Abstractions;
using UltrasoundAssistant.ProjectionService.Application.Common;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

namespace UltrasoundAssistant.ProjectionService.Application.EventHandlers;

public sealed class PatientCreatedEventHandler : IIntegrationEventHandler
{
    private readonly ProjectionDbContext _dbContext;
    private readonly ILogger<PatientCreatedEventHandler> _logger;

    public string RoutingKey => "patient.created";

    public PatientCreatedEventHandler(
        ProjectionDbContext dbContext,
        ILogger<PatientCreatedEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<PatientCreatedEvent>(payload, JsonDefaults.Web)
                     ?? throw new InvalidOperationException("Invalid PatientCreatedEvent payload");

        if (string.IsNullOrWhiteSpace(@event.FullName))
        {
            _logger.LogWarning(
                "Skipping patient.created because FullName is empty. Payload: {Payload}",
                payload);

            return;
        }

        var existing = await _dbContext.Patients
            .FirstOrDefaultAsync(x => x.Id == @event.Id, cancellationToken);

        if (existing is not null)
        {
            if (@event.Version <= existing.Version)
                return;

            existing.FullName = @event.FullName;
            existing.BirthDate = @event.BirthDate;
            existing.Gender = @event.Gender;
            existing.IsDeleted = false;
            existing.Version = @event.Version;
        }
        else
        {
            _dbContext.Patients.Add(new PatientReadModel
            {
                Id = @event.Id,
                FullName = @event.FullName,
                BirthDate = @event.BirthDate,
                Gender = @event.Gender,
                IsDeleted = false,
                Version = @event.Version
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}