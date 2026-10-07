using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using UP01_Faradzhov_Schedule.Models;

namespace UP01_Faradzhov_Schedule.Pages;

/// <summary>
/// Страница регистрации нового пользователя с комплексной проверкой надежности пароля и уникальности логина
/// </summary>
public partial class RegPage : Page
{
    public RegPage()
    {
        InitializeComponent();
    }

    private void btnSubmitRegister_Click(object sender, RoutedEventArgs e)
    {
        string fullName = tbFullName.Text.Trim();
        string login = tbLogin.Text.Trim();
        string email = tbEmail.Text.Trim();
        string phone = tbPhone.Text.Trim();
        string pass = pbPassword.Password;
        string confirmPass = pbConfirmPassword.Password;

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(pass))
        {
            MessageBox.Show("Пожалуйста, заполните все обязательные поля (ФИО, логин, пароль)!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (pass.Length < 6)
        {
            MessageBox.Show("Пароль должен содержать минимум 6 символов!", "Ошибка пароля", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!pass.Any(char.IsDigit))
        {
            MessageBox.Show("Пароль должен содержать хотя бы одну цифру!", "Ошибка пароля", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!Regex.IsMatch(pass, @"[a-zA-Z]"))
        {
            MessageBox.Show("Пароль должен содержать хотя бы одну латинскую букву!", "Ошибка пароля", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (pass != confirmPass)
        {
            MessageBox.Show("Введенные пароли не совпадают!", "Ошибка подтверждения", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = new ScheduleDbContext();

            if (db.UserAccounts.Any(u => u.Login.ToLower() == login.ToLower()))
            {
                MessageBox.Show($"Пользователь с логином «{login}» уже существует! Выберите другой логин.", "Логин занят", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newUser = new UserAccount
            {
                Login = login,
                PasswordHash = SecurityHelper.GetHash(pass),
                Role = "User",
                FullName = fullName,
                Email = string.IsNullOrWhiteSpace(email) ? null : email,
                Phone = string.IsNullOrWhiteSpace(phone) ? null : phone,
                Photo = "/res/default_avatar.png"
            };

            db.UserAccounts.Add(newUser);
            db.SaveChanges();

            MessageBox.Show("Регистрация успешно завершена! Теперь вы можете войти в систему.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService?.Navigate(new AuthPage());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.GoBack();
    }
}
