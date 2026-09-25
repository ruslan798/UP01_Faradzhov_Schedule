using System.Collections.Generic;

namespace UP01_Faradzhov_Schedule.Models;

/// <summary>
/// Отделение или кафедра
/// </summary>
public partial class Department
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? HeadName { get; set; }

    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();

    public virtual ICollection<StudyGroup> StudyGroups { get; set; } = new List<StudyGroup>();

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
