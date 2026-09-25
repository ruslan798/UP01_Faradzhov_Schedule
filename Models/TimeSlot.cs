using System.Collections.Generic;

namespace UP01_Faradzhov_Schedule.Models;

/// <summary>
/// Временной слот проведения учебного занятия (пара)
/// </summary>
public partial class TimeSlot
{
    public int Id { get; set; }

    public int PairNumber { get; set; }

    public string StartTime { get; set; } = null!;

    public string EndTime { get; set; } = null!;

    public string DisplayTime => $"{PairNumber} пара ({StartTime} - {EndTime})";

    public virtual ICollection<ScheduleItem> ScheduleItems { get; set; } = new List<ScheduleItem>();
}
