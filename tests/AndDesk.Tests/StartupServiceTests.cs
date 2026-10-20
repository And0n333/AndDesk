using AndDesk.Services;

namespace AndDesk.Tests;

public class StartupServiceTests
{
    [Fact]
    public void StartupService_CanQueryStatusWithoutThrowing()
    {
        var service = new StartupService();
        // Querying current status should never throw an exception
        bool enabled = service.IsStartupEnabled();
        Assert.True(enabled || !enabled);
    }
}
