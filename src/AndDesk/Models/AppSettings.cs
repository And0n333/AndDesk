namespace AndDesk.Models;

public class AppSettings
{
    public int SchemaVersion { get; set; } = 1;
    public bool LaunchAtLogin { get; set; } = false;
    public bool StartMinimized { get; set; } = false;
    public bool CloseToTray { get; set; } = true;
    public bool HideDesktopIconsOnLaunch { get; set; } = false;
    public bool RestoreIconsOnExit { get; set; } = true;
    public bool ShowHiddenFiles { get; set; } = false;
    public string ViewMode { get; set; } = "Grid"; // "Grid" or "List"
    public string SortMode { get; set; } = "Name"; // "Name", "Type", "Date"
}
