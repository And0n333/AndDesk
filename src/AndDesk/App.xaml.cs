using System.Windows;
using AndDesk.Infrastructure;
using AndDesk.Services;
using AndDesk.ViewModels;
using AndDesk.Views;

namespace AndDesk;

public partial class App : Application
{
    private SingleInstance? _singleInstance;
    private ISettingsService? _settingsService;
    private IDesktopVisibilityService? _visibilityService;
    private IDesktopWatcherService? _watcherService;
    private MainWindow? _mainWindow;

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        // 1. Initialize visibility & recovery services
        _visibilityService = new DesktopVisibilityService();
        var recoveryService = new RecoveryService(_visibilityService);

        // 2. Handle CLI recovery mode (--restore)
        if (recoveryService.CheckAndHandleCommandLine(e.Args))
        {
            Shutdown(0);
            return;
        }

        // 3. Single instance check
        _singleInstance = new SingleInstance();
        if (!_singleInstance.Start())
        {
            // Another instance is already running and has been signalled to wake up
            Shutdown(0);
            return;
        }

        _singleInstance.OnWakeupRequested += () =>
        {
            Dispatcher.Invoke(() =>
            {
                if (_mainWindow != null)
                {
                    if (_mainWindow.WindowState == WindowState.Minimized)
                    {
                        _mainWindow.WindowState = WindowState.Normal;
                    }
                    _mainWindow.Show();
                    _mainWindow.Activate();
                    _mainWindow.Focus();
                }
            });
        };

        // 4. Initialize Core Services
        _settingsService = new SettingsService();
        var shellService = new ShellService();
        var scanner = new DesktopScanner(shellService);
        var startupService = new StartupService();
        _watcherService = new DesktopWatcherService(scanner);

        var mainVm = new MainViewModel(
            scanner,
            shellService,
            _visibilityService,
            _settingsService,
            startupService,
            _watcherService
        );

        // 5. Create Main Window
        _mainWindow = new MainWindow(mainVm, _settingsService);

        // Optional launch auto-hide desktop icons
        if (_settingsService.CurrentSettings.HideDesktopIconsOnLaunch)
        {
            _visibilityService.HideDesktopIcons();
            mainVm.UpdateDesktopVisibilityState();
        }

        // 6. Show or minimize
        if (!_settingsService.CurrentSettings.StartMinimized)
        {
            _mainWindow.Show();
        }

        // 7. Initial desktop scan
        await mainVm.RefreshCommand.ExecuteAsync(null);

        // If started minimized, trim working set immediately after initial scan
        if (_settingsService.CurrentSettings.StartMinimized)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(300);
                MemoryOptimizer.TrimWorkingSet();
            });
        }
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        // Stop file system watcher
        _watcherService?.Dispose();

        // Restore icons on exit if configured and currently hidden
        if (_settingsService?.CurrentSettings.RestoreIconsOnExit == true)
        {
            _visibilityService?.RestoreDesktopIcons();
        }

        _singleInstance?.Dispose();
    }
}
