

namespace UltrasoundAssistant.Contracts.Reports
{
    /// <summary>
    /// Запрос на обновление поля отчёта.
    /// </summary>
    public class UpdateReportFieldRequest
    {
        /// <summary>
        /// Ожидаемая версия агрегата отчёта (optimistic concurrency).
        /// </summary>
        public int ExpectedVersion { get; set; }

        /// <summary>
        /// Имя поля.
        /// </summary>
        public string FieldName { get; set; } = null!;

        /// <summary>
        /// Новое значение поля.
        /// </summary>
        public string Value { get; set; } = null!;

        /// <summary>
        /// Уверенность (0–1), по умолчанию 1.
        /// </summary>
        public double Confidence { get; set; } = 1.0;
    }
}
