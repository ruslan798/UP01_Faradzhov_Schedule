using System.Collections.Generic;

namespace UP01_Faradzhov_Schedule.Models;

/// <summary>
/// Учебная группа студентов
/// </summary>
public partial class StudyGroup
{
    public int Id { get; set; }

    public string GroupName { get; set; } = null!;

    public int DepartmentId { get; set; }

    public int CourseNumber { get; set; }

    public int StudentCount { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<ScheduleItem> ScheduleItems { get; set; } = new List<ScheduleItem>();
}
