using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP01_Faradzhov_Schedule.Models;

namespace UP01_Faradzhov_Schedule.Pages;

/// <summary>
/// Страница добавления нового преподавателя в базу данных
/// </summary>
public partial class AddTeacherPage : Page
{
    public AddTeacherPage()
    {
        InitializeComponent();
        LoadDepartments();
    }

    private void LoadDepartments()
    {
        try
        {
            using var db = new ScheduleDbContext();
            cmbDepartment.ItemsSource = db.Departments.OrderBy(d => d.Name).ToList();
            if (cmbDepartment.Items.Count > 0) cmbDepartment.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки отделений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnSave_Click(object sender, RoutedEventArgs e)
    {
        string fullName = tbFullName.Text.Trim();
        int? deptId = cmbDepartment.SelectedValue as int?;
        string degree = tbDegree.Text.Trim();
        string email = tbEmail.Text.Trim();
        string phone = tbPhone.Text.Trim();

        if (string.IsNullOrWhiteSpace(fullName) || !deptId.HasValue || string.IsNullOrWhiteSpace(degree))
        {
            MessageBox.Show("Пожалуйста, заполните ФИО, отделение и должность/степень!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = new ScheduleDbContext();

            var teacher = new Teacher
            {
                FullName = fullName,
                DepartmentId = deptId.Value,
                AcademicDegree = degree,
                Email = string.IsNullOrWhiteSpace(email) ? null : email,
                Phone = string.IsNullOrWhiteSpace(phone) ? null : phone,
                Photo = "/res/default_avatar.png"
            };

            db.Teachers.Add(teacher);
            db.SaveChanges();

            MessageBox.Show("Преподаватель успешно добавлен в систему!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService?.GoBack();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения преподавателя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.GoBack();
    }
}
