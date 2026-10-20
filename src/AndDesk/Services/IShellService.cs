using System.Windows.Media;

namespace AndDesk.Services;

public interface IShellService
{
    (bool Success, string? ErrorMessage) OpenItem(string path);
    ImageSource? GetIcon(string path, bool isDirectory);
    string GetFriendlyTypeName(string path);
}
