using UltrasoundAssistant.Contracts.Events.Abstraction;

namespace UltrasoundAssistant.Contracts.Events.PatientEvent
{
    /// <summary>
    /// Событие создания пациента.
    /// </summary>
    public class PatientCreatedEvent : IEvent
    {
        /// <inheritdoc />
        public Guid EventId { get; set; } = Guid.NewGuid();

        /// <inheritdoc />
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Идентификатор пациента.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Полное имя пациента.
        /// </summary>
        public string FullName { get; set; } = null!;

        /// <summary>
        /// Дата рождения пациента.
        /// </summary>
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// Пол пациента.
        /// </summary>
        public string? Gender { get; set; }

        /// <summary>
        /// Версия агрегата (для event sourcing / concurrency).
        /// </summary>
        public int Version { get; set; } = 1;
    }
}
