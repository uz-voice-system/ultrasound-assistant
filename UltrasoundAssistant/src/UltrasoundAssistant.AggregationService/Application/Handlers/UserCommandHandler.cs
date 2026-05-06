using UltrasoundAssistant.AggregationService.Application.Abstractions;
using UltrasoundAssistant.AggregationService.Application.Common;
using UltrasoundAssistant.AggregationService.Application.Services;
using UltrasoundAssistant.AggregationService.Application.Validation;
using UltrasoundAssistant.AggregationService.Domain;
using UltrasoundAssistant.Contracts.Commands.Users;

namespace UltrasoundAssistant.AggregationService.Application.Handlers;

public sealed class UserCommandHandler : CommandHandlerBase
{
    private const string AggregateType = "user";
    private const string UserCreatedRoutingKey = "user.created";
    private const string UserUpdatedRoutingKey = "user.updated";
    private const string UserActivatedRoutingKey = "user.activated";
    private const string UserDeactivatedRoutingKey = "user.deactivated";

    private readonly IEventStore _eventStore;
    private readonly PasswordHashService _passwordHashService;

    public UserCommandHandler(
        IEventStore eventStore,
        IIntegrationEventPublisher publisher,
        IUnitOfWork unitOfWork,
        ILogger<UserCommandHandler> logger,
        PasswordHashService passwordHashService)
        : base(eventStore, publisher, unitOfWork, logger)
    {
        _eventStore = eventStore;
        _passwordHashService = passwordHashService;
    }

    public async Task<CommandResult> CreateAsync(CreateUserCommand command, CancellationToken ct)
    {
        try
        {
            UserCommandValidator.Validate(command);

            var aggregate = await LoadAggregateAsync(command.UserId, ct);

            var passwordHash = _passwordHashService.HashPassword(command.Password);

            var @event = aggregate.Create(
                command.UserId,
                command.Login,
                passwordHash,
                command.FullName,
                command.Role,
                command.DoctorProfile);

            return await SaveAndPublishAsync(
                AggregateType,
                command.UserId,
                aggregate.Version,
                [EventFactory.Create(@event, UserCreatedRoutingKey)],
                ct);
        }
        catch (ArgumentException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
        catch (DomainException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
    }

    public async Task<CommandResult> UpdateAsync(UpdateUserCommand command, CancellationToken ct)
    {
        try
        {
            UserCommandValidator.Validate(command);

            var aggregate = await LoadAggregateAsync(command.UserId, ct);

            if (!aggregate.Exists)
                return CommandResult.NotFound("User not found");

            if (command.ExpectedVersion != aggregate.Version)
            {
                return CommandResult.Conflict($"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
            }

            var passwordHash = string.IsNullOrWhiteSpace(command.Password)
                ? aggregate.PasswordHash
                : _passwordHashService.HashPassword(command.Password);

            var @event = aggregate.Update(
                command.Login,
                passwordHash,
                command.FullName,
                command.Role,
                command.IsActive,
                command.DoctorProfile);

            return await SaveAndPublishAsync(
                AggregateType,
                command.UserId,
                aggregate.Version,
                [EventFactory.Create(@event, UserUpdatedRoutingKey)],
                ct);
        }
        catch (ArgumentException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
        catch (DomainException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
    }

    public async Task<CommandResult> ActivateAsync(ActivateUserCommand command, CancellationToken ct)
    {
        try
        {
            UserCommandValidator.Validate(command);

            var aggregate = await LoadAggregateAsync(command.UserId, ct);

            if (!aggregate.Exists)
                return CommandResult.NotFound("User not found");

            if (command.ExpectedVersion != aggregate.Version)
            {
                return CommandResult.Conflict($"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
            }

            var @event = aggregate.Activate();

            return await SaveAndPublishAsync(
                AggregateType,
                command.UserId,
                aggregate.Version,
                [EventFactory.Create(@event, UserActivatedRoutingKey)],
                ct);
        }
        catch (ArgumentException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
        catch (DomainException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
    }

    public async Task<CommandResult> DeactivateAsync(DeactivateUserCommand command, CancellationToken ct)
    {
        try
        {
            UserCommandValidator.Validate(command);

            var aggregate = await LoadAggregateAsync(command.UserId, ct);

            if (!aggregate.Exists)
                return CommandResult.NotFound("User not found");

            if (command.ExpectedVersion != aggregate.Version)
            {
                return CommandResult.Conflict($"Concurrency conflict. Expected {command.ExpectedVersion}, actual {aggregate.Version}");
            }

            var @event = aggregate.Deactivate();

            return await SaveAndPublishAsync(
                AggregateType,
                command.UserId,
                aggregate.Version,
                [EventFactory.Create(@event, UserDeactivatedRoutingKey)],
                ct);
        }
        catch (ArgumentException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
        catch (DomainException ex)
        {
            return CommandResult.BadRequest(ex.Message);
        }
    }

    private async Task<UserAggregate> LoadAggregateAsync(Guid userId, CancellationToken ct)
    {
        var history = await _eventStore.LoadAggregateEventsAsync(
            AggregateType,
            userId,
            ct);

        var aggregate = new UserAggregate();
        aggregate.LoadFrom(history);

        return aggregate;
    }
}
