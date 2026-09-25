using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace UP01_Faradzhov_Schedule.Models;

/// <summary>
/// Вспомогательный класс криптографической защиты: хеширование паролей пользователей алгоритмом SHA-1
/// </summary>
public static class SecurityHelper
{
    /// <summary>
    /// Вычисляет шестнадцатеричный SHA-1 хеш пароля
    /// </summary>
    /// <param name="password">Исходный пароль</param>
    /// <returns>40-значный шестнадцатеричный хеш SHA-1</returns>
    public static string GetHash(string password)
    {
        if (string.IsNullOrEmpty(password)) return string.Empty;
        using var hash = SHA1.Create();
        return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
    }
}
