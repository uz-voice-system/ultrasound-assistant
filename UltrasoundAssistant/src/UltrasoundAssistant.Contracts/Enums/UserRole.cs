

using System.ComponentModel;

namespace UltrasoundAssistant.Contracts.Enums
{
    /// <summary>
    /// Роли пользователей в системе.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Врач-диагност.
        /// </summary>
        [Description("Врач")]
        Doctor,

        /// <summary>
        /// Администратор системы.
        /// </summary>
        [Description("Администратор")]
        Admin,

        /// <summary>
        /// Регистратор (ввод данных пациентов).
        /// </summary>
        [Description("Регистратор")]
        Registrar
    }
}
