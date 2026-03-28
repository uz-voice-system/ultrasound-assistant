using System.Collections;

namespace UltrasoundAssistant.Contracts.Common
{
    /// <summary>
    /// Ответ API с постраничной выборкой данных.
    /// </summary>
    public class PagedResponse<T> : BaseResponse<IEnumerable>
    {
        /// <summary>
        /// Общее количество элементов.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Текущая страница.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Размер страницы.
        /// </summary>
        public int PageSize { get; set; }
    }
}
