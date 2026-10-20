using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AndDesk.Infrastructure;
using AndDesk.Models;
using AndDesk.Services;
using AndDesk.ViewModels;

namespace AndDesk.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly ISettingsService _settingsService;
    private bool _isExplicitExit;

    public MainWindow(MainViewModel viewModel, ISettingsService settingsService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _settingsService = settingsService;
        DataContext = _viewModel;
    }

    private void DesktopItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            if (sender is FrameworkElement fe && fe.DataContext is DesktopItem item)
            {
                _viewModel.OpenItemCommand.Execute(item);
            }
        }
    }

    private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListView lv && lv.SelectedItem is DesktopItem item)
        {
            _viewModel.OpenItemCommand.Execute(item);
        }
    }

    private void AppTrayIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
    {
        if (IsVisible && WindowState != WindowState.Minimized)
        {
            Hide();
            ScheduleMemoryTrim();
        }
        else
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }
    }

    private void TrayOpenWindow_Click(object sender, RoutedEventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void TrayExit_Click(object sender, RoutedEventArgs e)
    {
        _isExplicitExit = true;
        Application.Current.Shutdown();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_isExplicitExit && _settingsService.CurrentSettings.CloseToTray)
        {
            e.Cancel = true;
            Hide();
            ScheduleMemoryTrim();
            return;
        }

        base.OnClosing(e);
    }

    private static void ScheduleMemoryTrim()
    {
        Task.Run(async () =>
        {
            // Delay 150ms to allow WPF window hide animation/transition to settle
            await Task.Delay(150);
            MemoryOptimizer.TrimWorkingSet();
        });
    }
}
