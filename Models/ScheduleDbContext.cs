using System;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;

namespace UP01_Faradzhov_Schedule.Models;

/// <summary>
/// Контекст базы данных сервиса автоматизированного составления расписания
/// </summary>
public partial class ScheduleDbContext : DbContext
{
    public ScheduleDbContext()
    {
    }

    public ScheduleDbContext(DbContextOptions<ScheduleDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<UserAccount> UserAccounts { get; set; } = null!;
    public virtual DbSet<Department> Departments { get; set; } = null!;
    public virtual DbSet<Teacher> Teachers { get; set; } = null!;
    public virtual DbSet<Classroom> Classrooms { get; set; } = null!;
    public virtual DbSet<StudyGroup> StudyGroups { get; set; } = null!;
    public virtual DbSet<Subject> Subjects { get; set; } = null!;
    public virtual DbSet<TimeSlot> TimeSlots { get; set; } = null!;
    public virtual DbSet<ScheduleItem> ScheduleItems { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(GetConnectionString());
        }
    }

    /// <summary>
    /// Автоматическое определение строки подключения: Docker (127.0.0.1,1433) или локальный экземпляр MS SQL Server
    /// </summary>
    public static string GetConnectionString()
    {
        var env = Environment.GetEnvironmentVariable("SCHEDULE_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(env))
        {
            return env;
        }

        try
        {
            using var tcp = new TcpClient();
            var ar = tcp.BeginConnect("127.0.0.1", 1433, null, null);
            if (ar.AsyncWaitHandle.WaitOne(300))
            {
                tcp.EndConnect(ar);
                return "Server=127.0.0.1,1433;Database=DB_Faradzhov_Schedule;User Id=sa;Password=YourStrong@Passw0rd!;TrustServerCertificate=True;";
            }
        }
        catch
        {
        }

        return "Server=.;Database=DB_Faradzhov_Schedule;Trusted_Connection=True;TrustServerCertificate=True;";
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("user_account");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Login).HasColumnName("login").HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(50);
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(255);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(50);
            entity.Property(e => e.Photo).HasColumnName("photo");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("department");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255);
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(50);
            entity.Property(e => e.HeadName).HasColumnName("head_name").HasMaxLength(255);
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.ToTable("teacher");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(255);
            entity.Property(e => e.AcademicDegree).HasColumnName("academic_degree").HasMaxLength(150);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(50);
            entity.Property(e => e.Photo).HasColumnName("photo");

            entity.HasOne(d => d.User)
                .WithMany(p => p.Teachers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Department)
                .WithMany(p => p.Teachers)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Classroom>(entity =>
        {
            entity.ToTable("classroom");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RoomNumber).HasColumnName("room_number").HasMaxLength(50);
            entity.Property(e => e.Building).HasColumnName("building").HasMaxLength(100);
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.RoomType).HasColumnName("room_type").HasMaxLength(100);
            entity.Property(e => e.HasProjector).HasColumnName("has_projector");
        });

        modelBuilder.Entity<StudyGroup>(entity =>
        {
            entity.ToTable("study_group");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GroupName).HasColumnName("group_name").HasMaxLength(100);
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.CourseNumber).HasColumnName("course_number");
            entity.Property(e => e.StudentCount).HasColumnName("student_count");

            entity.HasOne(d => d.Department)
                .WithMany(p => p.StudyGroups)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("subject");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255);
            entity.Property(e => e.TotalHours).HasColumnName("total_hours");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");

            entity.HasOne(d => d.Department)
                .WithMany(p => p.Subjects)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.ToTable("time_slot");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PairNumber).HasColumnName("pair_number");
            entity.Property(e => e.StartTime).HasColumnName("start_time").HasMaxLength(10);
            entity.Property(e => e.EndTime).HasColumnName("end_time").HasMaxLength(10);
        });

        modelBuilder.Entity<ScheduleItem>(entity =>
        {
            entity.ToTable("schedule_item");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week").HasMaxLength(50);
            entity.Property(e => e.TimeSlotId).HasColumnName("time_slot_id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");
            entity.Property(e => e.TeacherId).HasColumnName("teacher_id");
            entity.Property(e => e.ClassroomId).HasColumnName("classroom_id");
            entity.Property(e => e.LessonType).HasColumnName("lesson_type").HasMaxLength(50);
            entity.Property(e => e.WeekType).HasColumnName("week_type").HasMaxLength(50);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(d => d.TimeSlot)
                .WithMany(p => p.ScheduleItems)
                .HasForeignKey(d => d.TimeSlotId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Group)
                .WithMany(p => p.ScheduleItems)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Subject)
                .WithMany(p => p.ScheduleItems)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Teacher)
                .WithMany(p => p.ScheduleItems)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Classroom)
                .WithMany(p => p.ScheduleItems)
                .HasForeignKey(d => d.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
