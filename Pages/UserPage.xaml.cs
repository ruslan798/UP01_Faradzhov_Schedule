using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using UP01_Faradzhov_Schedule.Models;

namespace UP01_Faradzhov_Schedule.Pages;

/// <summary>
/// Страница пользователя: просмотр расписания, карточек преподавателей и аудиторного фонда
/// </summary>
public partial class UserPage : Page
{
    private readonly UserAccount? _user;
    private List<ScheduleItem> _allScheduleItems = new();
    private List<Teacher> _allTeachers = new();

    public UserPage(UserAccount? user = null)
    {
        InitializeComponent();
        _user = user;
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            using var db = new ScheduleDbContext();

            _allScheduleItems = db.ScheduleItems
                .Include(s => s.TimeSlot)
                .Include(s => s.Group)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Include(s => s.Classroom)
                .AsNoTracking()
                .ToList();

            _allTeachers = db.Teachers
                .Include(t => t.Department)
                .AsNoTracking()
                .OrderBy(t => t.FullName)
                .ToList();

            var classrooms = db.Classrooms.AsNoTracking().OrderBy(c => c.RoomNumber).ToList();
            dgClassrooms.ItemsSource = classrooms;

            ApplyFilters();
            ApplyTeacherFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки данных расписания: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void FilterChanged(object sender, RoutedEventArgs e)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        if (!IsInitialized) return;

        var query = _allScheduleItems.AsEnumerable();

        // 1. Поиск
        string search = tbSearch.Text.Trim().ToLower();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.Group.GroupName.ToLower().Contains(search) ||
                s.Subject.Name.ToLower().Contains(search) ||
                s.Teacher.FullName.ToLower().Contains(search) ||
                s.Classroom.RoomNumber.ToLower().Contains(search));
        }

        // 2. Фильтр по дню недели
        string selectedDay = (cmbDayFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Все дни недели";
        if (selectedDay != "Все дни недели")
        {
            query = query.Where(s => s.DayOfWeek == selectedDay);
        }

        // 3. Фильтр по типу недели
        string selectedWeek = (cmbWeekFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Все недели";
        if (selectedWeek != "Все недели")
        {
            query = query.Where(s => s.WeekType == selectedWeek || s.WeekType == "Все недели");
        }

        // 4. Только лекции
        if (cbOnlyLectures.IsChecked == true)
        {
            query = query.Where(s => s.LessonType == "Лекция");
        }

        // 5. Сортировка
        int sortIndex = cmbSort.SelectedIndex;
        query = sortIndex switch
        {
            1 => query.OrderBy(s => s.Group.GroupName).ThenBy(s => s.TimeSlot.PairNumber),
            2 => query.OrderBy(s => s.Subject.Name).ThenBy(s => s.TimeSlot.PairNumber),
            3 => query.OrderBy(s => s.Teacher.FullName).ThenBy(s => s.TimeSlot.PairNumber),
            _ => query.OrderBy(s => GetDayOrder(s.DayOfWeek)).ThenBy(s => s.TimeSlot.PairNumber)
        };

        var filteredList = query.ToList();
        dgSchedule.ItemsSource = filteredList;
        txtRecordCount.Text = $"Найдено: {filteredList.Count} из {_allScheduleItems.Count}";
    }

    private static int GetDayOrder(string day) => day switch
    {
        "Понедельник" => 1,
        "Вторник" => 2,
        "Среда" => 3,
        "Четверг" => 4,
        "Пятница" => 5,
        "Суббота" => 6,
        _ => 7
    };

    private void btnResetFilters_Click(object sender, RoutedEventArgs e)
    {
        tbSearch.Text = string.Empty;
        cmbDayFilter.SelectedIndex = 0;
        cmbWeekFilter.SelectedIndex = 0;
        cmbSort.SelectedIndex = 0;
        cbOnlyLectures.IsChecked = false;
        ApplyFilters();
    }

    private void tbTeacherSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyTeacherFilter();
    }

    private void ApplyTeacherFilter()
    {
        if (!IsInitialized) return;

        string search = tbTeacherSearch.Text.Trim().ToLower();
        var teachers = string.IsNullOrWhiteSpace(search)
            ? _allTeachers
            : _allTeachers.Where(t => t.FullName.ToLower().Contains(search) || t.AcademicDegree.ToLower().Contains(search)).ToList();

        lvTeachers.ItemsSource = teachers;
        txtTeacherCount.Text = $"Всего: {teachers.Count} из {_allTeachers.Count}";
    }
}
