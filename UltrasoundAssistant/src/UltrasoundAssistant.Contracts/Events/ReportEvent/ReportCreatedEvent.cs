using UltrasoundAssistant.Contracts.Enums;
using UltrasoundAssistant.Contracts.Events.Abstraction;

namespace UltrasoundAssistant.Contracts.Events.ReportEvent
{
    /// <summary>
    /// Событие создания отчёта.
    /// </summary>
    public class ReportCreatedEvent : IEvent
    {
        /// <inheritdoc />
        public Guid EventId { get; set; } = Guid.NewGuid();

        /// <inheritdoc />
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Идентификатор отчёта.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор пациента.
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// Идентификатор врача.
        /// </summary>
        public Guid DoctorId { get; set; }

        /// <summary>
        /// Идентификатор шаблона.
        /// </summary>
        public Guid TemplateId { get; set; }

        /// <summary>
        /// Текущий статус отчёта.
        /// </summary>
        public ReportStatus Status { get; set; }

        /// <summary>
        /// Версия агрегата.
        /// </summary>
        public int Version { get; set; } = 1;
    }
}
