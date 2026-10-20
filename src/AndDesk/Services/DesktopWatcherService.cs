using System.Diagnostics;
using System.IO;
using System.Timers;

namespace AndDesk.Services;

public class DesktopWatcherService : IDesktopWatcherService
{
    private readonly IDesktopScanner _scanner;
    private readonly List<FileSystemWatcher> _watchers = new();
    private readonly System.Timers.Timer _debounceTimer;

    public event Action? OnDesktopChanged;

    public DesktopWatcherService(IDesktopScanner scanner)
    {
        _scanner = scanner;

        // 400ms debounce timer to prevent continuous scans during batch file operations
        _debounceTimer = new System.Timers.Timer(400)
        {
            AutoReset = false
        };
        _debounceTimer.Elapsed += (s, e) =>
        {
            OnDesktopChanged?.Invoke();
        };
    }

    public void Start()
    {
        Stop();

        var directories = _scanner.GetDesktopDirectories();
        foreach (var dir in directories)
        {
            try
            {
                if (!Directory.Exists(dir)) continue;

                var watcher = new FileSystemWatcher(dir)
                {
                    NotifyFilter = NotifyFilters.FileName |
                                   NotifyFilters.DirectoryName |
                                   NotifyFilters.LastWrite,
                    IncludeSubdirectories = false,
                    EnableRaisingEvents = true
                };

                watcher.Created += OnFileSystemEvent;
                watcher.Deleted += OnFileSystemEvent;
                watcher.Renamed += OnFileSystemEvent;
                watcher.Changed += OnFileSystemEvent;

                _watchers.Add(watcher);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to watch directory {dir}: {ex.Message}");
            }
        }
    }

    public void Stop()
    {
        _debounceTimer.Stop();
        foreach (var watcher in _watchers)
        {
            try
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
        }
        _watchers.Clear();
    }

    private void OnFileSystemEvent(object sender, FileSystemEventArgs e)
    {
        // Ignore desktop.ini and temp files
        string name = e.Name ?? string.Empty;
        if (name.Equals("desktop.ini", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("thumbs.db", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("~$"))
        {
            return;
        }

        // Reset debounce timer
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    public void Dispose()
    {
        Stop();
        _debounceTimer.Dispose();
    }
}
