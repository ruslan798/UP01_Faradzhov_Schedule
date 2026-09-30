using System;
using System.Windows;
using PdfSharpCore.Fonts;

namespace UP01_Faradzhov_Schedule;

/// <summary>
/// Логика взаимодействия для App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            GlobalFontSettings.FontResolver = new FileFontResolver();
        }
        catch
        {
        }
    }
}
