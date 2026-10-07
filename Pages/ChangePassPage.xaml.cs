using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using UP01_Faradzhov_Schedule.Models;

namespace UP01_Faradzhov_Schedule.Pages;

/// <summary>
/// Страница изменения пароля пользователя с криптографической проверкой старого пароля
/// </summary>
public partial class ChangePassPage : Page
{
    public ChangePassPage(string? prefilledLogin = null)
    {
        InitializeComponent();
        if (!string.IsNullOrWhiteSpace(prefilledLogin))
        {
            tbLogin.Text = prefilledLogin;
        }
    }

    private void btnSavePassword_Click(object sender, RoutedEventArgs e)
    {
        string login = tbLogin.Text.Trim();
        string oldPass = pbOldPassword.Password;
        string newPass = pbNewPassword.Password;
        string confirmNewPass = pbConfirmNewPassword.Password;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(oldPass) || string.IsNullOrWhiteSpace(newPass))
        {
            MessageBox.Show("Пожалуйста, заполните логин, текущий и новый пароль!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (newPass.Length < 6 || !newPass.Any(char.IsDigit) || !Regex.IsMatch(newPass, @"[a-zA-Z]"))
        {
            MessageBox.Show("Новый пароль должен содержать минимум 6 знаков, латинские буквы и цифры!", "Слабый пароль", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (newPass != confirmNewPass)
        {
            MessageBox.Show("Новый пароль и его подтверждение не совпадают!", "Ошибка подтверждения", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = new ScheduleDbContext();
            string oldHash = SecurityHelper.GetHash(oldPass);

            var user = db.UserAccounts.FirstOrDefault(u => u.Login == login && u.PasswordHash == oldHash);
            if (user == null)
            {
                MessageBox.Show("Пользователь с указанным логином и паролем не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (oldPass == newPass)
            {
                MessageBox.Show("Новый пароль не должен совпадать с текущим!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            user.PasswordHash = SecurityHelper.GetHash(newPass);
            db.SaveChanges();

            MessageBox.Show("Пароль успешно обновлен! Авторизуйтесь с новым паролем.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService?.Navigate(new AuthPage());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка обновления пароля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.GoBack();
    }
}
