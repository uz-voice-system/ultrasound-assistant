

namespace UltrasoundAssistant.Contracts.Patients
{
    /// <summary>
    /// Запрос на создание пациента.
    /// </summary>
    public class CreatePatientRequest
    {
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
    }
}
