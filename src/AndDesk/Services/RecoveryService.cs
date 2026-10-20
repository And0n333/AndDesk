namespace AndDesk.Services;

public class RecoveryService
{
    private readonly IDesktopVisibilityService _visibilityService;

    public RecoveryService(IDesktopVisibilityService visibilityService)
    {
        _visibilityService = visibilityService;
    }

    public bool CheckAndHandleCommandLine(string[] args)
    {
        if (args.Any(arg => arg.Equals("--restore", StringComparison.OrdinalIgnoreCase) ||
                            arg.Equals("-restore", StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("[AndDesk] 正在执行桌面图标恢复模式 (--restore)...");
            bool success = _visibilityService.RestoreDesktopIcons();
            if (success)
            {
                Console.WriteLine("[AndDesk] 桌面图标已成功恢复。");
            }
            else
            {
                Console.WriteLine("[AndDesk] 警告：未能确认桌面图标恢复状态，请在桌面空白处右键查看->显示桌面图标。");
            }
            return true;
        }

        return false;
    }
}
