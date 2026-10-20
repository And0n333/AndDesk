using System.Runtime.InteropServices;

namespace AndDesk.Services;

public class DesktopVisibilityService : IDesktopVisibilityService
{
    // Windows Explorer internal command ID to toggle "Show desktop icons"
    private const int WM_COMMAND = 0x0111;
    private const int TOGGLE_DESKTOP_ICONS_CMD = 0x7402; // 29698

    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

    public IntPtr GetDesktopDefViewHandle()
    {
        // 1. Try Progman first
        IntPtr hProgman = FindWindow("Progman", null);
        if (hProgman != IntPtr.Zero)
        {
            IntPtr hDefView = FindWindowEx(hProgman, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (hDefView != IntPtr.Zero)
            {
                return hDefView;
            }
        }

        // 2. In Windows 11 (especially with wallpaper slide/transition), SHELLDLL_DefView is under a WorkerW window
        IntPtr hFound = IntPtr.Zero;
        EnumWindows((hWnd, lParam) =>
        {
            IntPtr hDefView = FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (hDefView != IntPtr.Zero)
            {
                hFound = hDefView;
                return false; // Stop enumeration
            }
            return true;
        }, IntPtr.Zero);

        return hFound;
    }

    public IntPtr GetDesktopListViewHandle(IntPtr hDefView)
    {
        if (hDefView == IntPtr.Zero) return IntPtr.Zero;
        return FindWindowEx(hDefView, IntPtr.Zero, "SysListView32", null);
    }

    public bool? IsDesktopIconsVisible()
    {
        IntPtr hDefView = GetDesktopDefViewHandle();
        if (hDefView == IntPtr.Zero) return null;

        if (!IsWindowVisible(hDefView))
        {
            return false;
        }

        IntPtr hListView = GetDesktopListViewHandle(hDefView);
        if (hListView != IntPtr.Zero)
        {
            return IsWindowVisible(hListView);
        }

        return true;
    }

    public bool HideDesktopIcons()
    {
        IntPtr hDefView = GetDesktopDefViewHandle();
        if (hDefView == IntPtr.Zero)
        {
            System.Diagnostics.Debug.WriteLine("Desktop DefView not found.");
            return false;
        }

        bool? visible = IsDesktopIconsVisible();
        if (visible == false)
        {
            return true; // Already hidden
        }

        // Primary approach: Send WM_COMMAND (0x7402)
        SendMessage(hDefView, WM_COMMAND, (IntPtr)TOGGLE_DESKTOP_ICONS_CMD, IntPtr.Zero);
        Thread.Sleep(80);

        visible = IsDesktopIconsVisible();
        if (visible == false)
        {
            return true;
        }

        // Fallback: ShowWindow SW_HIDE
        ShowWindow(hDefView, SW_HIDE);
        Thread.Sleep(50);
        return IsDesktopIconsVisible() == false;
    }

    public bool RestoreDesktopIcons()
    {
        IntPtr hDefView = GetDesktopDefViewHandle();
        if (hDefView == IntPtr.Zero)
        {
            System.Diagnostics.Debug.WriteLine("Desktop DefView not found.");
            return false;
        }

        // 1. Ensure window itself is not SW_HIDE
        ShowWindow(hDefView, SW_SHOW);

        bool? visible = IsDesktopIconsVisible();
        if (visible == true)
        {
            return true; // Already visible
        }

        // Send WM_COMMAND to toggle back
        SendMessage(hDefView, WM_COMMAND, (IntPtr)TOGGLE_DESKTOP_ICONS_CMD, IntPtr.Zero);
        Thread.Sleep(80);

        return IsDesktopIconsVisible() == true;
    }

    public bool ToggleDesktopIcons()
    {
        bool? current = IsDesktopIconsVisible();
        if (current == false)
        {
            return RestoreDesktopIcons();
        }
        else
        {
            return HideDesktopIcons();
        }
    }

    #region Win32 P/Invoke

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string? lpszWindow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    #endregion
}
