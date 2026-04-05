

namespace UltrasoundAssistant.Contracts.Reports
{
    /// <summary>
    /// Запрос на создание отчёта.
    /// </summary>
    public class CreateReportRequest
    {
        /// <summary>
        /// Идентификатор отчёта (если не задан — генерируется на шлюзе).
        /// </summary>
        public Guid? ReportId { get; set; }

        /// <summary>
        /// Идентификатор пациента.
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// Идентификатор врача (пользователя).
        /// </summary>
        public Guid DoctorId { get; set; }

        /// <summary>
        /// Идентификатор шаблона.
        /// </summary>
        public Guid TemplateId { get; set; }
    }
}
