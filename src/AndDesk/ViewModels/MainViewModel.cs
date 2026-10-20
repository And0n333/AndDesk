using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AndDesk.Models;
using AndDesk.Services;
using AndDesk.Views;

namespace AndDesk.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IDesktopScanner _scanner;
    private readonly IShellService _shellService;
    private readonly IDesktopVisibilityService _visibilityService;
    private readonly ISettingsService _settingsService;
    private readonly IStartupService _startupService;
    private readonly IDesktopWatcherService _watcherService;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "就绪";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isDesktopHidden;

    [ObservableProperty]
    private string _viewMode = "Grid"; // "Grid" or "List"

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _filteredCount;

    public ObservableCollection<DesktopItem> Items { get; } = new();
    public ObservableCollection<DesktopItem> FilteredItems { get; } = new();

    public MainViewModel(
        IDesktopScanner scanner,
        IShellService shellService,
        IDesktopVisibilityService visibilityService,
        ISettingsService settingsService,
        IStartupService startupService,
        IDesktopWatcherService watcherService)
    {
        _scanner = scanner;
        _shellService = shellService;
        _visibilityService = visibilityService;
        _settingsService = settingsService;
        _startupService = startupService;
        _watcherService = watcherService;

        _viewMode = _settingsService.CurrentSettings.ViewMode;
        UpdateDesktopVisibilityState();

        // Bind desktop file watcher
        _watcherService.OnDesktopChanged += () =>
        {
            Application.Current?.Dispatcher.InvokeAsync(async () =>
            {
                await RefreshAsync();
            });
        };
        _watcherService.Start();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    public void UpdateDesktopVisibilityState()
    {
        bool? visible = _visibilityService.IsDesktopIconsVisible();
        if (visible.HasValue)
        {
            IsDesktopHidden = !visible.Value;
        }
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        StatusMessage = "正在扫描桌面项目...";

        try
        {
            var scanned = await _scanner.ScanAsync(_settingsService.CurrentSettings.ShowHiddenFiles);

            Items.Clear();
            foreach (var item in scanned.OrderBy(i => i.IsDirectory ? 0 : 1).ThenBy(i => i.Name))
            {
                Items.Add(item);
            }

            TotalCount = Items.Count;
            ApplyFilter();
            UpdateDesktopVisibilityState();

            StatusMessage = $"就绪 (共 {TotalCount} 个项目)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"扫描异常: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void OpenItem(DesktopItem? item)
    {
        if (item == null) return;

        var (success, errorMsg) = _shellService.OpenItem(item.FullPath);
        if (!success)
        {
            StatusMessage = $"[错误] {errorMsg}";
        }
        else
        {
            StatusMessage = $"已打开: {item.Name}";
        }
    }

    [RelayCommand]
    public void HideDesktopIcons()
    {
        StatusMessage = "正在隐藏 Windows 桌面图标...";
        bool success = _visibilityService.HideDesktopIcons();
        UpdateDesktopVisibilityState();

        if (success)
        {
            StatusMessage = "Windows 桌面图标已隐藏（真实文件未改动）";
        }
        else
        {
            StatusMessage = "[警告] 无法隐藏桌面图标，请确认当前 Explorer 状态";
        }
    }

    [RelayCommand]
    public void RestoreDesktopIcons()
    {
        StatusMessage = "正在恢复 Windows 桌面图标...";
        bool success = _visibilityService.RestoreDesktopIcons();
        UpdateDesktopVisibilityState();

        if (success)
        {
            StatusMessage = "Windows 桌面图标已恢复显示";
        }
        else
        {
            StatusMessage = "[警告] 无法恢复桌面图标，请在桌面空白处右键查看->显示桌面图标";
        }
    }

    [RelayCommand]
    public void ToggleDesktopIcons()
    {
        if (IsDesktopHidden)
        {
            RestoreDesktopIcons();
        }
        else
        {
            HideDesktopIcons();
        }
    }

    [RelayCommand]
    public void ToggleViewMode()
    {
        ViewMode = ViewMode == "Grid" ? "List" : "Grid";
        _settingsService.UpdateSettings(s => s.ViewMode = ViewMode);
    }

    [RelayCommand]
    public void OpenSettings()
    {
        var settingsVm = new SettingsViewModel(_settingsService, _startupService);
        settingsVm.OnSettingsChanged += () =>
        {
            _ = RefreshAsync();
        };

        var window = new SettingsWindow(settingsVm)
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();
    }

    private void ApplyFilter()
    {
        FilteredItems.Clear();

        var query = Items.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(i => i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                     i.DisplayType.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var item in query)
        {
            FilteredItems.Add(item);
        }

        FilteredCount = FilteredItems.Count;
    }
}
