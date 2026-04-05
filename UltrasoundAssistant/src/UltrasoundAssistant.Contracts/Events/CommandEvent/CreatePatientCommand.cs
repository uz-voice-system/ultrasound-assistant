

namespace UltrasoundAssistant.Contracts.Events.CommandEvent
{
    /// <summary>
    /// Команда на создание пациента.
    /// </summary>
    public class CreatePatientCommand
    {
        /// <summary>
        /// Уникальный идентификатор пациента (генерируется на уровне API).
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
        /// Пол пациента (опционально).
        /// </summary>
        public string? Gender { get; set; }
    }
}
