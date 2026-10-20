using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AndDesk.Infrastructure;

public static class MemoryOptimizer
{
    [DllImport("psapi.dll", SetLastError = true)]
    private static extern int EmptyWorkingSet(IntPtr hwProc);

    /// <summary>
    /// Trims the process working set by collecting GC and releasing unused physical pages back to Windows.
    /// Call this when the application enters background/tray state.
    /// </summary>
    public static void TrimWorkingSet()
    {
        try
        {
            // 1. Collect managed garbage
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // 2. Request Windows OS to trim working set
            EmptyWorkingSet(Process.GetCurrentProcess().Handle);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to trim working set: {ex.Message}");
        }
    }
}
