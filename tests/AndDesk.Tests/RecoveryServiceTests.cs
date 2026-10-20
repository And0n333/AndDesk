using AndDesk.Services;

namespace AndDesk.Tests;

public class MockDesktopVisibilityService : IDesktopVisibilityService
{
    public bool RestoreCalled { get; private set; }
    public bool HideCalled { get; private set; }

    public bool? IsDesktopIconsVisible() => true;

    public bool HideDesktopIcons()
    {
        HideCalled = true;
        return true;
    }

    public bool RestoreDesktopIcons()
    {
        RestoreCalled = true;
        return true;
    }

    public bool ToggleDesktopIcons() => true;
}

public class RecoveryServiceTests
{
    [Theory]
    [InlineData("--restore")]
    [InlineData("-restore")]
    [InlineData("--RESTORE")]
    public void CheckAndHandleCommandLine_TriggersRecovery_WhenRestoreFlagPresent(string flag)
    {
        var mockVisibility = new MockDesktopVisibilityService();
        var recoveryService = new RecoveryService(mockVisibility);

        bool handled = recoveryService.CheckAndHandleCommandLine(new[] { flag });

        Assert.True(handled);
        Assert.True(mockVisibility.RestoreCalled);
    }

    [Theory]
    [InlineData("")]
    [InlineData("--minimized")]
    [InlineData("-open")]
    public void CheckAndHandleCommandLine_Ignores_WhenNoRestoreFlag(string arg)
    {
        var mockVisibility = new MockDesktopVisibilityService();
        var recoveryService = new RecoveryService(mockVisibility);

        string[] args = string.IsNullOrEmpty(arg) ? Array.Empty<string>() : new[] { arg };
        bool handled = recoveryService.CheckAndHandleCommandLine(args);

        Assert.False(handled);
        Assert.False(mockVisibility.RestoreCalled);
    }
}
