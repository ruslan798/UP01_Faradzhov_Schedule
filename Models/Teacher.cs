using System.Collections.Generic;

namespace UP01_Faradzhov_Schedule.Models;

/// <summary>
/// Преподаватель или сотрудник образовательного учреждения
/// </summary>
public partial class Teacher
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int DepartmentId { get; set; }

    public string FullName { get; set; } = null!;

    public string AcademicDegree { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Photo { get; set; }

    public virtual UserAccount? User { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<ScheduleItem> ScheduleItems { get; set; } = new List<ScheduleItem>();
}
