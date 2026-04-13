using UltrasoundAssistant.Contracts.Commands.Templates;

namespace UltrasoundAssistant.AggregationService.Application.Validation;

public static class TemplateCommandValidator
{
    public static void Validate(CreateTemplateCommand command)
    {
        if (command.CommandId == Guid.Empty)
            throw new ArgumentException("CommandId is required");

        if (command.TemplateId == Guid.Empty)
            throw new ArgumentException("TemplateId is required");

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ArgumentException("Template name is required");

        if (command.Keywords is null || command.Keywords.Count == 0)
            throw new ArgumentException("Keywords are required");
    }

    public static void Validate(UpdateTemplateCommand command)
    {
        if (command.CommandId == Guid.Empty)
            throw new ArgumentException("CommandId is required");

        if (command.TemplateId == Guid.Empty)
            throw new ArgumentException("TemplateId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");
    }

    public static void Validate(DeleteTemplateCommand command)
    {
        if (command.CommandId == Guid.Empty)
            throw new ArgumentException("CommandId is required");

        if (command.TemplateId == Guid.Empty)
            throw new ArgumentException("TemplateId is required");

        if (command.ExpectedVersion < 0)
            throw new ArgumentException("ExpectedVersion cannot be negative");
    }
}