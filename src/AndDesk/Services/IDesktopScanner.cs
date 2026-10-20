using AndDesk.Models;

namespace AndDesk.Services;

public interface IDesktopScanner
{
    Task<List<DesktopItem>> ScanAsync(bool showHiddenFiles = false);
    IReadOnlyList<string> GetDesktopDirectories();
}
