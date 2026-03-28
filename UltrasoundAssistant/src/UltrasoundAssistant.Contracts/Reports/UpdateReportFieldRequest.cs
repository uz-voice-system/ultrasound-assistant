

namespace UltrasoundAssistant.Contracts.Reports
{
    /// <summary>
    /// Запрос на обновление поля отчёта.
    /// </summary>
    public class UpdateReportFieldRequest
    {
        /// <summary>
        /// Имя поля.
        /// </summary>
        public string FieldName { get; set; } = null!;

        /// <summary>
        /// Новое значение поля.
        /// </summary>
        public string Value { get; set; } = null!;
    }
}
