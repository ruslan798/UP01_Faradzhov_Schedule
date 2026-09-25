using System;

namespace UP01_Faradzhov_Schedule.Models;

/// <summary>
/// Элемент расписания: занятие для группы, дисциплины, преподавателя и аудитории
/// </summary>
public partial class ScheduleItem
{
    public int Id { get; set; }

    public string DayOfWeek { get; set; } = null!;

    public int TimeSlotId { get; set; }

    public int GroupId { get; set; }

    public int SubjectId { get; set; }

    public int TeacherId { get; set; }

    public int ClassroomId { get; set; }

    public string LessonType { get; set; } = null!;

    public string WeekType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual TimeSlot TimeSlot { get; set; } = null!;

    public virtual StudyGroup Group { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;

    public virtual Teacher Teacher { get; set; } = null!;

    public virtual Classroom Classroom { get; set; } = null!;
}
