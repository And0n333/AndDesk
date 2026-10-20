using AndDesk.Services;
using System.IO;

namespace AndDesk.Tests;

public class MockShellService : IShellService
{
    public (bool Success, string? ErrorMessage) OpenItem(string path) => (true, null);
    public System.Windows.Media.ImageSource? GetIcon(string path, bool isDirectory) => null;
    public string GetFriendlyTypeName(string path) => "测试文件";
}

public class ScannerTests
{
    [Fact]
    public void GetDesktopDirectories_ReturnsValidDirectories()
    {
        var mockShell = new MockShellService();
        var scanner = new DesktopScanner(mockShell);

        var dirs = scanner.GetDesktopDirectories();

        Assert.NotEmpty(dirs);
        foreach (var dir in dirs)
        {
            Assert.True(Directory.Exists(dir));
        }
    }
}
