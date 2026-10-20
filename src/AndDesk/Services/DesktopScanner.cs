using System.IO;
using AndDesk.Models;

namespace AndDesk.Services;

public class DesktopScanner : IDesktopScanner
{
    private readonly IShellService _shellService;

    // Common system/desktop internal files to exclude
    private static readonly HashSet<string> ExcludedFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "desktop.ini",
        "thumbs.db",
        "ntuser.dat",
        "ntuser.ini"
    };

    public DesktopScanner(IShellService shellService)
    {
        _shellService = shellService;
    }

    public IReadOnlyList<string> GetDesktopDirectories()
    {
        var dirs = new List<string>();

        // Current user desktop (handles OneDrive redirection automatically)
        string userDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        if (Directory.Exists(userDesktop))
        {
            dirs.Add(userDesktop);
        }

        string userDesktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (Directory.Exists(userDesktopDirectory) && !dirs.Contains(userDesktopDirectory, StringComparer.OrdinalIgnoreCase))
        {
            dirs.Add(userDesktopDirectory);
        }

        // Public / Common desktop
        string commonDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
        if (Directory.Exists(commonDesktop) && !dirs.Contains(commonDesktop, StringComparer.OrdinalIgnoreCase))
        {
            dirs.Add(commonDesktop);
        }

        return dirs;
    }

    public async Task<List<DesktopItem>> ScanAsync(bool showHiddenFiles = false)
    {
        return await Task.Run(() =>
        {
            var items = new List<DesktopItem>();
            var scannedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var directories = GetDesktopDirectories();

            foreach (var dir in directories)
            {
                bool isPublic = dir.Contains("Public", StringComparison.OrdinalIgnoreCase);
                string sourceLabel = isPublic ? "公共桌面" : "个人桌面";

                try
                {
                    var dirInfo = new DirectoryInfo(dir);
                    if (!dirInfo.Exists) continue;

                    // 1. Enumerate Directories
                    foreach (var subDir in dirInfo.EnumerateDirectories())
                    {
                        try
                        {
                            if (scannedPaths.Contains(subDir.FullName)) continue;

                            if (!showHiddenFiles)
                            {
                                if ((subDir.Attributes & FileAttributes.Hidden) != 0 ||
                                    (subDir.Attributes & FileAttributes.System) != 0)
                                {
                                    continue;
                                }
                            }

                            scannedPaths.Add(subDir.FullName);

                            var item = new DesktopItem
                            {
                                Name = subDir.Name,
                                FullPath = subDir.FullName,
                                IsDirectory = true,
                                ItemKind = DesktopItemKind.Directory,
                                DisplayType = "文件夹",
                                SourceFolder = sourceLabel,
                                LastModified = subDir.LastWriteTime,
                                Icon = _shellService.GetIcon(subDir.FullName, isDirectory: true)
                            };

                            items.Add(item);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error scanning subDir {subDir.FullName}: {ex.Message}");
                        }
                    }

                    // 2. Enumerate Files
                    foreach (var file in dirInfo.EnumerateFiles())
                    {
                        try
                        {
                            if (scannedPaths.Contains(file.FullName)) continue;

                            // Skip system internal files
                            if (ExcludedFileNames.Contains(file.Name) || file.Name.StartsWith("~$"))
                            {
                                continue;
                            }

                            if (!showHiddenFiles)
                            {
                                if ((file.Attributes & FileAttributes.Hidden) != 0 ||
                                    (file.Attributes & FileAttributes.System) != 0)
                                {
                                    continue;
                                }
                            }

                            scannedPaths.Add(file.FullName);

                            string ext = file.Extension.ToLowerInvariant();
                            DesktopItemKind kind = DesktopItemKind.File;
                            string displayName = file.Name;

                            if (ext == ".lnk")
                            {
                                kind = DesktopItemKind.Shortcut;
                                displayName = Path.GetFileNameWithoutExtension(file.Name);
                            }
                            else if (ext == ".url")
                            {
                                kind = DesktopItemKind.Shortcut;
                                displayName = Path.GetFileNameWithoutExtension(file.Name);
                            }

                            var item = new DesktopItem
                            {
                                Name = displayName,
                                FullPath = file.FullName,
                                IsDirectory = false,
                                ItemKind = kind,
                                DisplayType = _shellService.GetFriendlyTypeName(file.FullName),
                                FileSizeBytes = file.Length,
                                SourceFolder = sourceLabel,
                                LastModified = file.LastWriteTime,
                                Icon = _shellService.GetIcon(file.FullName, isDirectory: false)
                            };

                            items.Add(item);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error scanning file {file.FullName}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error scanning directory {dir}: {ex.Message}");
                }
            }

            return items;
        });
    }
}
