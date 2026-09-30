using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using UP01_Faradzhov_Schedule.Models;
using UP01_Faradzhov_Schedule.Pages;

namespace UP01_Faradzhov_Schedule;

/// <summary>
/// Главное окно приложения: координация навигации, таймер, динамическая смена тем и сессия пользователя
/// </summary>
public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
    private bool _isDarkTheme;
    public static UserAccount? CurrentUser { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (s, e) =>
        {
            TimeTB.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        };
        _timer.Start();

        MainFrame.Navigate(new AuthPage());
    }

    /// <summary>
    /// Переключение между светлой (Dictionary.xaml) и темной (CustomTheme.xaml) темами оформления
    /// </summary>
    private void btnThemeToggle_Click(object sender, RoutedEventArgs e)
    {
        _isDarkTheme = !_isDarkTheme;
        string themeUri = _isDarkTheme ? "CustomTheme.xaml" : "Dictionary.xaml";

        var dict = new ResourceDictionary
        {
            Source = new Uri(themeUri, UriKind.Relative)
        };

        Application.Current.Resources.MergedDictionaries.Clear();
        Application.Current.Resources.MergedDictionaries.Add(dict);

        btnThemeToggle.Content = _isDarkTheme ? "☀️ Светлая" : "🌙 Тёмная";
    }

    /// <summary>
    /// Возврат на предыдущую страницу в стеке навигации
    /// </summary>
    private void btnBack_Click(object sender, RoutedEventArgs e)
    {
        if (MainFrame.CanGoBack)
        {
            MainFrame.GoBack();
        }
    }

    /// <summary>
    /// Обновление динамического заголовка и видимости кнопок при навигации
    /// </summary>
    private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
    {
        btnBack.Visibility = MainFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;

        if (e.Content is Page page)
        {
            TitleTB.Text = string.IsNullOrWhiteSpace(page.Title) ? "Расписание занятий" : page.Title;
        }

        if (CurrentUser != null)
        {
            UserTB.Text = $"{CurrentUser.Role}: {CurrentUser.FullName}";
            btnLogout.Visibility = Visibility.Visible;
        }
        else
        {
            UserTB.Text = "Вход в систему";
            btnLogout.Visibility = Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Завершение сеанса работы и переход на окно авторизации
    /// </summary>
    private void btnLogout_Click(object sender, RoutedEventArgs e)
    {
        var res = MessageBox.Show("Вы действительно хотите выйти из учетной записи?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (res == MessageBoxResult.Yes)
        {
            CurrentUser = null;
            MainFrame.Navigate(new AuthPage());
        }
    }

    /// <summary>
    /// Запрос подтверждения перед закрытием приложения
    /// </summary>
    private void Window_Closing(object sender, CancelEventArgs e)
    {
        var result = MessageBox.Show("Вы действительно хотите выйти из приложения?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes)
        {
            e.Cancel = true;
        }
    }
}
