

namespace UltrasoundAssistant.Contracts.Reports
{
    /// <summary>
    /// Запрос на создание отчёта.
    /// </summary>
    public class CreateReportRequest
    {
        /// <summary>
        /// Идентификатор пациента.
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// Идентификатор шаблона.
        /// </summary>
        public Guid TemplateId { get; set; }
    }
}
