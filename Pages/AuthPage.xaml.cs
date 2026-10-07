using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP01_Faradzhov_Schedule.Models;

namespace UP01_Faradzhov_Schedule.Pages;

/// <summary>
/// Страница аутентификации пользователей: ввод учетных данных, сверка SHA-1 хеша и защита капчей
/// </summary>
public partial class AuthPage : Page
{
    private int _failedAttempts;

    public AuthPage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Авторизация пользователя в системе
    /// </summary>
    private void btnEnter_Click(object sender, RoutedEventArgs e)
    {
        string login = tbLogin.Text.Trim();
        string pass = pbPassword.Password;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(pass))
        {
            MessageBox.Show("Пожалуйста, заполните все поля логина и пароля!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = new ScheduleDbContext();
            string hash = SecurityHelper.GetHash(pass);

            var user = db.UserAccounts.FirstOrDefault(u => u.Login == login && u.PasswordHash == hash);
            if (user != null)
            {
                MainWindow.CurrentUser = user;
                _failedAttempts = 0;

                MessageBox.Show($"Добро пожаловать в систему, {user.FullName}!", "Успешная авторизация", MessageBoxButton.OK, MessageBoxImage.Information);

                if (user.Role == "Admin")
                {
                    NavigationService?.Navigate(new AdminPage());
                }
                else
                {
                    NavigationService?.Navigate(new UserPage(user));
                }
            }
            else
            {
                _failedAttempts++;
                if (_failedAttempts >= 3)
                {
                    MessageBox.Show("Вы трижды неверно ввели данные. В целях безопасности пройдите проверку капчей!", "Защита", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CaptchaSwitch(true);
                    CaptchaChange();
                }
                else
                {
                    MessageBox.Show($"Неверный логин или пароль! (Попытка {_failedAttempts} из 3)", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Включение и отключение панели капчи с блокировкой кнопки входа
    /// </summary>
    public void CaptchaSwitch(bool showCaptcha)
    {
        if (showCaptcha)
        {
            captchaPanel.Visibility = Visibility.Visible;
            btnEnter.IsEnabled = false;
        }
        else
        {
            captchaPanel.Visibility = Visibility.Collapsed;
            btnEnter.IsEnabled = true;
            tbCaptchaInput.Clear();
        }
    }

    /// <summary>
    /// Генерация случайной строки символов капчи из 6 знаков
    /// </summary>
    public void CaptchaChange()
    {
        string allowChar = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,y,z,1,2,3,4,5,6,7,8,9,0";
        string[] ar = allowChar.Split(',');
        string pwd = "";
        var r = new Random();

        for (int i = 0; i < 6; i++)
        {
            pwd += ar[r.Next(0, ar.Length)];
        }

        txtCaptcha.Text = pwd;
    }

    private void btnRefreshCaptcha_Click(object sender, RoutedEventArgs e)
    {
        CaptchaChange();
    }

    /// <summary>
    /// Проверка введенного пользователем защитного кода
    /// </summary>
    private void btnConfirmCaptcha_Click(object sender, RoutedEventArgs e)
    {
        if (string.Equals(tbCaptchaInput.Text.Trim(), txtCaptcha.Text.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Капча успешно пройдена! Можете повторить попытку входа.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            CaptchaSwitch(false);
            _failedAttempts = 0;
        }
        else
        {
            MessageBox.Show("Неверно введена капча! Попробуйте снова.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            CaptchaChange();
            tbCaptchaInput.Clear();
        }
    }

    /// <summary>
    /// Блокировка системных команд буфера обмена (Copy, Cut, Paste) в поле ввода капчи
    /// </summary>
    private void tbCaptchaInput_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Command == ApplicationCommands.Copy ||
            e.Command == ApplicationCommands.Cut ||
            e.Command == ApplicationCommands.Paste)
        {
            e.Handled = true;
        }
    }

    private void btnRegister_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new RegPage());
    }

    private void btnChangePassword_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new ChangePassPage(tbLogin.Text.Trim()));
    }
}
