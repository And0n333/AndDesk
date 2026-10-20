using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndDesk.Models;
using AndDesk.Services;

namespace AndDesk.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IStartupService _startupService;

    [ObservableProperty]
    private bool _launchAtLogin;

    [ObservableProperty]
    private bool _startMinimized;

    [ObservableProperty]
    private bool _closeToTray;

    [ObservableProperty]
    private bool _hideDesktopIconsOnLaunch;

    [ObservableProperty]
    private bool _restoreIconsOnExit;

    [ObservableProperty]
    private bool _showHiddenFiles;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public event Action? OnSettingsChanged;

    public SettingsViewModel(ISettingsService settingsService, IStartupService startupService)
    {
        _settingsService = settingsService;
        _startupService = startupService;

        LoadCurrentSettings();
    }

    private void LoadCurrentSettings()
    {
        var settings = _settingsService.CurrentSettings;
        StartMinimized = settings.StartMinimized;
        CloseToTray = settings.CloseToTray;
        HideDesktopIconsOnLaunch = settings.HideDesktopIconsOnLaunch;
        RestoreIconsOnExit = settings.RestoreIconsOnExit;
        ShowHiddenFiles = settings.ShowHiddenFiles;

        // Sync startup setting with actual registry status
        LaunchAtLogin = _startupService.IsStartupEnabled();
    }

    partial void OnLaunchAtLoginChanged(bool value)
    {
        bool success = _startupService.SetStartup(value);
        if (success)
        {
            _settingsService.UpdateSettings(s => s.LaunchAtLogin = value);
            StatusMessage = value ? "已启用开机自启动" : "已禁用开机自启动";
        }
        else
        {
            StatusMessage = "[警告] 无法修改开机启动项，请检查权限";
        }
        OnSettingsChanged?.Invoke();
    }

    partial void OnStartMinimizedChanged(bool value)
    {
        _settingsService.UpdateSettings(s => s.StartMinimized = value);
        OnSettingsChanged?.Invoke();
    }

    partial void OnCloseToTrayChanged(bool value)
    {
        _settingsService.UpdateSettings(s => s.CloseToTray = value);
        OnSettingsChanged?.Invoke();
    }

    partial void OnHideDesktopIconsOnLaunchChanged(bool value)
    {
        _settingsService.UpdateSettings(s => s.HideDesktopIconsOnLaunch = value);
        OnSettingsChanged?.Invoke();
    }

    partial void OnRestoreIconsOnExitChanged(bool value)
    {
        _settingsService.UpdateSettings(s => s.RestoreIconsOnExit = value);
        OnSettingsChanged?.Invoke();
    }

    partial void OnShowHiddenFilesChanged(bool value)
    {
        _settingsService.UpdateSettings(s => s.ShowHiddenFiles = value);
        OnSettingsChanged?.Invoke();
    }

    [RelayCommand]
    public void ResetDefaults()
    {
        _settingsService.UpdateSettings(s =>
        {
            s.LaunchAtLogin = false;
            s.StartMinimized = false;
            s.CloseToTray = true;
            s.HideDesktopIconsOnLaunch = false;
            s.RestoreIconsOnExit = true;
            s.ShowHiddenFiles = false;
        });

        _startupService.SetStartup(false);
        LoadCurrentSettings();
        StatusMessage = "已恢复默认设置";
        OnSettingsChanged?.Invoke();
    }
}
