using System.Diagnostics;
using Microsoft.Win32;

namespace AndDesk.Services;

public class StartupService : IStartupService
{
    private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppValueName = "AndDesk";

    public bool IsStartupEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false);
            if (key == null) return false;

            var val = key.GetValue(AppValueName) as string;
            if (string.IsNullOrWhiteSpace(val)) return false;

            // Verify if the registered path matches current exe
            string currentPath = GetExecutablePath();
            return val.Contains(currentPath, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to check startup registry: {ex.Message}");
            return false;
        }
    }

    public bool SetStartup(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true);
            if (key == null) return false;

            if (enable)
            {
                string exePath = GetExecutablePath();
                string command = $"\"{exePath}\"";
                key.SetValue(AppValueName, command);
            }
            else
            {
                if (key.GetValue(AppValueName) != null)
                {
                    key.DeleteValue(AppValueName, false);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to update startup registry: {ex.Message}");
            return false;
        }
    }

    private static string GetExecutablePath()
    {
        return Environment.ProcessPath 
               ?? Process.GetCurrentProcess().MainModule?.FileName 
               ?? AppDomain.CurrentDomain.BaseDirectory;
    }
}
