using System.Windows.Media;

namespace AndDesk.Models;

public enum DesktopItemKind
{
    File,
    Directory,
    Shortcut,
    Unknown
}

public class DesktopItem
{
    public required string Name { get; set; }
    public required string FullPath { get; set; }
    public bool IsDirectory { get; set; }
    public DesktopItemKind ItemKind { get; set; }
    public ImageSource? Icon { get; set; }
    public bool Exists { get; set; } = true;
    public string? SourceFolder { get; set; } // "User" or "Public"
    public string DisplayType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime LastModified { get; set; }

    public string Subtitle => IsDirectory ? "文件夹" : DisplayType;
}
