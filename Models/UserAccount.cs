using System.Collections.Generic;

namespace UP01_Faradzhov_Schedule.Models;

/// <summary>
/// Учетная запись пользователя системы
/// </summary>
public partial class UserAccount
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Photo { get; set; }

    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}
