using System.Windows;
using AndDesk.Services;

namespace AndDeskRecovery;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var visibilityService = new DesktopVisibilityService();

        bool success = visibilityService.RestoreDesktopIcons();

        if (success)
        {
            MessageBox.Show(
                "Windows 桌面图标已成功恢复显示！",
                "AndDesk 桌面图标恢复工具",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
        else
        {
            MessageBox.Show(
                "未能自动确认桌面图标状态。\n\n如桌面图标仍未显示，请在桌面空白处点击鼠标右键 ->【查看】-> 勾选【显示桌面图标】。",
                "AndDesk 桌面图标恢复工具",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }
    }
}
