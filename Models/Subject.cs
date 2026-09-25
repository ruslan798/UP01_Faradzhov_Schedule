using System.Collections.Generic;

namespace UP01_Faradzhov_Schedule.Models;

/// <summary>
/// Учебная дисциплина, предмет или модуль
/// </summary>
public partial class Subject
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int TotalHours { get; set; }

    public int DepartmentId { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<ScheduleItem> ScheduleItems { get; set; } = new List<ScheduleItem>();
}
