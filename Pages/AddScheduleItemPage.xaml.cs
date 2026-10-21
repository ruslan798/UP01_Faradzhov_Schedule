using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP01_Faradzhov_Schedule.Models;

namespace UP01_Faradzhov_Schedule.Pages;

/// <summary>
/// Страница создания и редактирования занятия в расписании с автоматической валидацией конфликтов (пересечений)
/// </summary>
public partial class AddScheduleItemPage : Page
{
    private readonly ScheduleItem? _editingItem;

    public AddScheduleItemPage(ScheduleItem? editingItem = null)
    {
        InitializeComponent();
        _editingItem = editingItem;
        LoadSelectors();

        if (_editingItem != null)
        {
            txtHeader.Text = "Редактирование занятия расписания";
            btnSave.Content = "Сохранить изменения";
            FillExistingData(_editingItem);
        }
    }

    private void LoadSelectors()
    {
        try
        {
            using var db = new ScheduleDbContext();

            cmbSlot.ItemsSource = db.TimeSlots.OrderBy(s => s.PairNumber).ToList();
            if (cmbSlot.Items.Count > 0) cmbSlot.SelectedIndex = 0;

            cmbGroup.ItemsSource = db.StudyGroups.OrderBy(g => g.GroupName).ToList();
            if (cmbGroup.Items.Count > 0) cmbGroup.SelectedIndex = 0;

            cmbSubject.ItemsSource = db.Subjects.OrderBy(s => s.Name).ToList();
            if (cmbSubject.Items.Count > 0) cmbSubject.SelectedIndex = 0;

            cmbTeacher.ItemsSource = db.Teachers.OrderBy(t => t.FullName).ToList();
            if (cmbTeacher.Items.Count > 0) cmbTeacher.SelectedIndex = 0;

            cmbClassroom.ItemsSource = db.Classrooms.OrderBy(c => c.RoomNumber).ToList();
            if (cmbClassroom.Items.Count > 0) cmbClassroom.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки справочников: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void FillExistingData(ScheduleItem item)
    {
        foreach (ComboBoxItem cbi in cmbDay.Items)
        {
            if (cbi.Content?.ToString() == item.DayOfWeek)
            {
                cmbDay.SelectedItem = cbi;
                break;
            }
        }

        cmbSlot.SelectedValue = item.TimeSlotId;
        cmbGroup.SelectedValue = item.GroupId;
        cmbSubject.SelectedValue = item.SubjectId;
        cmbTeacher.SelectedValue = item.TeacherId;
        cmbClassroom.SelectedValue = item.ClassroomId;

        foreach (ComboBoxItem cbi in cmbLessonType.Items)
        {
            if (cbi.Content?.ToString() == item.LessonType)
            {
                cmbLessonType.SelectedItem = cbi;
                break;
            }
        }

        foreach (ComboBoxItem cbi in cmbWeekType.Items)
        {
            if (cbi.Content?.ToString() == item.WeekType)
            {
                cmbWeekType.SelectedItem = cbi;
                break;
            }
        }
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
        string day = (cmbDay.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Понедельник";
        int? slotId = cmbSlot.SelectedValue as int?;
        int? groupId = cmbGroup.SelectedValue as int?;
        int? subjectId = cmbSubject.SelectedValue as int?;
        int? teacherId = cmbTeacher.SelectedValue as int?;
        int? roomId = cmbClassroom.SelectedValue as int?;
        string lessonType = (cmbLessonType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Лекция";
        string weekType = (cmbWeekType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Все недели";

        if (!slotId.HasValue || !groupId.HasValue || !subjectId.HasValue || !teacherId.HasValue || !roomId.HasValue)
        {
            MessageBox.Show("Пожалуйста, выберите все параметры занятия!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = new ScheduleDbContext();

            // 1. Проверка коллизии: не занята ли аудитория в этот день и пару?
            var roomConflict = db.ScheduleItems
                .Where(s => (_editingItem == null || s.Id != _editingItem.Id)
                            && s.DayOfWeek == day
                            && s.TimeSlotId == slotId.Value
                            && s.ClassroomId == roomId.Value
                            && (s.WeekType == "Все недели" || weekType == "Все недели" || s.WeekType == weekType))
                .FirstOrDefault();

            if (roomConflict != null)
            {
                MessageBox.Show($"Конфликт расписания! Выбранная аудитория уже занята в {day} на {slotId.Value} паре!", "Коллизия аудитории", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Проверка коллизии: не ведет ли преподаватель другую пару в это же время?
            var teacherConflict = db.ScheduleItems
                .Where(s => (_editingItem == null || s.Id != _editingItem.Id)
                            && s.DayOfWeek == day
                            && s.TimeSlotId == slotId.Value
                            && s.TeacherId == teacherId.Value
                            && (s.WeekType == "Все недели" || weekType == "Все недели" || s.WeekType == weekType))
                .FirstOrDefault();

            if (teacherConflict != null)
            {
                MessageBox.Show($"Конфликт расписания! Данный преподаватель уже назначен на другое занятие в {day} на {slotId.Value} паре!", "Коллизия преподавателя", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Проверка коллизии: нет ли у группы уже занятия на этой паре?
            var groupConflict = db.ScheduleItems
                .Where(s => (_editingItem == null || s.Id != _editingItem.Id)
                            && s.DayOfWeek == day
                            && s.TimeSlotId == slotId.Value
                            && s.GroupId == groupId.Value
                            && (s.WeekType == "Все недели" || weekType == "Все недели" || s.WeekType == weekType))
                .FirstOrDefault();

            if (groupConflict != null)
            {
                MessageBox.Show($"Конфликт расписания! У группы уже стоит занятие в {day} на {slotId.Value} паре!", "Коллизия группы", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Сохранение
            if (_editingItem == null)
            {
                var newItem = new ScheduleItem
                {
                    DayOfWeek = day,
                    TimeSlotId = slotId.Value,
                    GroupId = groupId.Value,
                    SubjectId = subjectId.Value,
                    TeacherId = teacherId.Value,
                    ClassroomId = roomId.Value,
                    LessonType = lessonType,
                    WeekType = weekType,
                    Status = "Утверждено",
                    CreatedAt = DateTime.Now
                };
                db.ScheduleItems.Add(newItem);
            }
            else
            {
                var existing = db.ScheduleItems.Find(_editingItem.Id);
                if (existing != null)
                {
                    existing.DayOfWeek = day;
                    existing.TimeSlotId = slotId.Value;
                    existing.GroupId = groupId.Value;
                    existing.SubjectId = subjectId.Value;
                    existing.TeacherId = teacherId.Value;
                    existing.ClassroomId = roomId.Value;
                    existing.LessonType = lessonType;
                    existing.WeekType = weekType;
                }
            }

            db.SaveChanges();
            MessageBox.Show("Занятие успешно сохранено в расписании!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService?.GoBack();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения занятия: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.GoBack();
    }
}
