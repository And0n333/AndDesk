namespace AndDesk.Services;

public interface IDesktopVisibilityService
{
    bool? IsDesktopIconsVisible();
    bool HideDesktopIcons();
    bool RestoreDesktopIcons();
    bool ToggleDesktopIcons();
}
