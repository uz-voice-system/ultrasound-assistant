using UltrasoundAssistant.Contracts.Entity.Patients;
using UltrasoundAssistant.Contracts.Reads.Patients.Details;
using UltrasoundAssistant.Contracts.Reads.Patients.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities.Patients;

namespace UltrasoundAssistant.ProjectionService.Application.Mapping;

public sealed class PatientProjectionMapper
{
    public PatientDto MapFull(PatientReadModel model)
    {
        return new PatientDto
        {
            Id = model.Id,
            FullName = model.FullName,
            BirthDate = model.BirthDate,
            Gender = model.Gender,
            PhoneNumber = model.PhoneNumber,
            Email = model.Email,
            IsDeleted = model.IsDeleted,
            Version = model.Version,
            Documents = model.Documents.Select(MapDocument).ToList()
        };
    }

    public PatientSummaryDto MapSummary(PatientReadModel model)
    {
        return new PatientSummaryDto
        {
            Id = model.Id,
            FullName = model.FullName,
            BirthDate = model.BirthDate,
            Gender = model.Gender,
            PhoneNumber = model.PhoneNumber,
            IsDeleted = model.IsDeleted,
            Version = model.Version
        };
    }

    private static PatientDocumentDto MapDocument(PatientDocumentReadModel model)
    {
        return new PatientDocumentDto
        {
            Id = model.Id,
            DocumentType = model.DocumentType,
            Series = model.Series,
            Number = model.Number,
            IssuedBy = model.IssuedBy,
            IssueDate = model.IssueDate,
            DepartmentCode = model.DepartmentCode,
            Organization = model.Organization
        };
    }
}
