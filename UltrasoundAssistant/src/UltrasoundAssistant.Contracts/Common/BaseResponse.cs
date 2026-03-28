

namespace UltrasoundAssistant.Contracts.Common
{
    /// <summary>
    /// Базовый ответ API без данных.
    /// </summary>
    public class BaseResponse
    {
        /// <summary>
        /// Признак успешного выполнения запроса.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Сообщение об ошибке, если операция завершилась неудачно.
        /// </summary>
        public string? Error { get; set; }
    }

    /// <summary>
    /// Универсальный ответ API с данными.
    /// </summary>
    /// <typeparam name="T">Тип возвращаемых данных.</typeparam>
    public class BaseResponse<T> : BaseResponse
    {
        /// <summary>
        /// Данные, возвращаемые в ответе.
        /// </summary>
        public T? Data { get; set; }
    }
}
