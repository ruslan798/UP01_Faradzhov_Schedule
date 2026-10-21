using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using UP01_Faradzhov_Schedule.Models;

namespace UP01_Faradzhov_Schedule.Pages;

/// <summary>
/// Страница панели администратора: комплексное управление расписанием, преподавателями и справочниками
/// </summary>
public partial class AdminPage : Page
{
    public AdminPage()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            using var db = new ScheduleDbContext();

            dgSchedule.ItemsSource = db.ScheduleItems
                .Include(s => s.TimeSlot)
                .Include(s => s.Group)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Include(s => s.Classroom)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.TimeSlot.PairNumber)
                .ToList();

            dgTeachers.ItemsSource = db.Teachers
                .Include(t => t.Department)
                .OrderBy(t => t.FullName)
                .ToList();

            dgGroups.ItemsSource = db.StudyGroups
                .Include(g => g.Department)
                .OrderBy(g => g.GroupName)
                .ToList();

            dgRooms.ItemsSource = db.Classrooms
                .OrderBy(c => c.RoomNumber)
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки данных администрирования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnAddSchedule_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new AddScheduleItemPage());
    }

    private void btnEditSchedule_Click(object sender, RoutedEventArgs e)
    {
        if (dgSchedule.SelectedItem is not ScheduleItem selected)
        {
            MessageBox.Show("Пожалуйста, выберите занятие из таблицы для редактирования!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        NavigationService?.Navigate(new AddScheduleItemPage(selected));
    }

    private void btnDeleteSchedule_Click(object sender, RoutedEventArgs e)
    {
        if (dgSchedule.SelectedItem is not ScheduleItem selected)
        {
            MessageBox.Show("Пожалуйста, выберите занятие для удаления!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirm = MessageBox.Show($"Вы действительно хотите удалить занятие «{selected.Subject.Name}» ({selected.Group.GroupName}, {selected.DayOfWeek}, пара {selected.TimeSlot.PairNumber})?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            using var db = new ScheduleDbContext();
            var item = db.ScheduleItems.Find(selected.Id);
            if (item != null)
            {
                db.ScheduleItems.Remove(item);
                db.SaveChanges();
                MessageBox.Show("Занятие успешно удалено из расписания!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnAddTeacher_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new AddTeacherPage());
    }

    private void btnDeleteTeacher_Click(object sender, RoutedEventArgs e)
    {
        if (dgTeachers.SelectedItem is not Teacher selected)
        {
            MessageBox.Show("Пожалуйста, выберите преподавателя для удаления!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirm = MessageBox.Show($"Вы действительно хотите удалить преподавателя {selected.FullName}?\nВНИМАНИЕ: Все назначенные ему занятия также будут каскадно удалены из расписания!", "Каскадное удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            using var db = new ScheduleDbContext();
            var item = db.Teachers.Find(selected.Id);
            if (item != null)
            {
                db.Teachers.Remove(item);
                db.SaveChanges();
                MessageBox.Show("Преподаватель успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnGoToDiagrams_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new DiagrammPage());
    }
}
