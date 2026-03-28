using UltrasoundAssistant.Contracts.Events.Abstraction;

namespace UltrasoundAssistant.Contracts.Events.ReportEvent
{
    /// <summary>
    /// Событие обновления поля отчёта.
    /// Генерируется после обработки голосовых данных или ручного редактирования.
    /// </summary>
    public class ReportFieldUpdatedEvent : IEvent
    {
        /// <inheritdoc />
        public Guid EventId { get; set; } = Guid.NewGuid();

        /// <inheritdoc />
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Идентификатор отчёта.
        /// </summary>
        public Guid ReportId { get; set; }

        /// <summary>
        /// Имя поля.
        /// </summary>
        public string FieldName { get; set; } = null!;

        /// <summary>
        /// Значение поля.
        /// </summary>
        public string Value { get; set; } = null!;

        /// <summary>
        /// Уверенность распознавания.
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Версия агрегата.
        /// </summary>
        public int Version { get; set; }
    }
}
