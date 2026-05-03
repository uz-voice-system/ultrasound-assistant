using UltrasoundAssistant.Contracts.Reads.Reports.Details;
using UltrasoundAssistant.Contracts.Reads.Reports.Search;
using UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Entities;

namespace UltrasoundAssistant.ProjectionService.Application.Mapping;

public sealed class ReportProjectionMapper
{
    public ReportDto MapFull(ReportReadModel model)
    {
        return new ReportDto
        {
            Id = model.Id,
            AppointmentId = model.AppointmentId,
            PatientId = model.Appointment.PatientId,
            PatientFullName = model.Appointment.Patient.FullName,
            DoctorId = model.Appointment.DoctorId,
            DoctorFullName = model.Appointment.Doctor.FullName,
            TemplateId = model.Appointment.TemplateId,
            TemplateName = model.Appointment.Template.Name,
            Status = model.Status,
            ContentJson = model.ContentJson,
            IsDeleted = model.IsDeleted,
            CreatedAtUtc = model.CreatedAtUtc,
            UpdatedAtUtc = model.UpdatedAtUtc,
            Version = model.Version
        };
    }

    public ReportSummaryDto MapSummary(ReportReadModel model)
    {
        return new ReportSummaryDto
        {
            Id = model.Id,
            AppointmentId = model.AppointmentId,
            PatientFullName = model.Appointment.Patient.FullName,
            DoctorFullName = model.Appointment.Doctor.FullName,
            TemplateName = model.Appointment.Template.Name,
            AppointmentStartAtUtc = model.Appointment.StartAtUtc,
            Status = model.Status,
            CreatedAtUtc = model.CreatedAtUtc,
            Version = model.Version
        };
    }
}
