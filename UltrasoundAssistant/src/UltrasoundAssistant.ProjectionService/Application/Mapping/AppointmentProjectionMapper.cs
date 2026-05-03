using UltrasoundAssistant.Contracts.Reads.Appointments.Details;
using UltrasoundAssistant.Contracts.Reads.Appointments.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

namespace UltrasoundAssistant.ProjectionService.Application.Mapping;

public sealed class AppointmentProjectionMapper
{
    public AppointmentDto MapFull(AppointmentReadModel model)
    {
        return new AppointmentDto
        {
            Id = model.Id,
            PatientId = model.PatientId,
            PatientFullName = model.Patient.FullName,
            DoctorId = model.DoctorId,
            DoctorFullName = model.Doctor.FullName,
            TemplateId = model.TemplateId,
            TemplateName = model.Template.Name,
            CreatedByUserId = model.CreatedByUserId,
            CreatedByUserFullName = model.CreatedByUser.FullName,
            StartAtUtc = model.StartAtUtc,
            EndAtUtc = model.EndAtUtc,
            Status = model.Status,
            Comment = model.Comment,
            ReportId = model.Report?.Id,
            IsDeleted = model.IsDeleted,
            CreatedAtUtc = model.CreatedAtUtc,
            UpdatedAtUtc = model.UpdatedAtUtc,
            Version = model.Version
        };
    }

    public AppointmentSummaryDto MapSummary(AppointmentReadModel model)
    {
        return new AppointmentSummaryDto
        {
            Id = model.Id,
            PatientId = model.PatientId,
            PatientFullName = model.Patient.FullName,
            DoctorId = model.DoctorId,
            DoctorFullName = model.Doctor.FullName,
            TemplateId = model.TemplateId,
            TemplateName = model.Template.Name,
            StartAtUtc = model.StartAtUtc,
            EndAtUtc = model.EndAtUtc,
            Status = model.Status,
            ReportId = model.Report?.Id,
            Version = model.Version
        };
    }
}
