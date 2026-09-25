using System.Collections.Generic;

namespace UP01_Faradzhov_Schedule.Models;

/// <summary>
/// Учебная аудитория, кабинет или лаборатория
/// </summary>
public partial class Classroom
{
    public int Id { get; set; }

    public string RoomNumber { get; set; } = null!;

    public string Building { get; set; } = null!;

    public int Capacity { get; set; }

    public string RoomType { get; set; } = null!;

    public bool HasProjector { get; set; }

    public virtual ICollection<ScheduleItem> ScheduleItems { get; set; } = new List<ScheduleItem>();
}
