using AndDesk.Models;
using AndDesk.Services;
using System.IO;

namespace AndDesk.Tests;

public class ModelsAndSettingsTests
{
    [Fact]
    public void DesktopItem_Subtitle_ReturnsDirectoryForFolder()
    {
        var item = new DesktopItem
        {
            Name = "MyFolder",
            FullPath = @"C:\Users\User\Desktop\MyFolder",
            IsDirectory = true,
            ItemKind = DesktopItemKind.Directory,
            DisplayType = "文件夹"
        };

        Assert.Equal("文件夹", item.Subtitle);
    }

    [Fact]
    public void DesktopItem_Subtitle_ReturnsDisplayTypeForFile()
    {
        var item = new DesktopItem
        {
            Name = "Doc1.pdf",
            FullPath = @"C:\Users\User\Desktop\Doc1.pdf",
            IsDirectory = false,
            ItemKind = DesktopItemKind.File,
            DisplayType = "PDF 文档"
        };

        Assert.Equal("PDF 文档", item.Subtitle);
    }

    [Fact]
    public void SettingsService_SavesAndLoadsDefaultsSuccessfully()
    {
        var service = new SettingsService();
        Assert.NotNull(service.CurrentSettings);
        Assert.Equal(1, service.CurrentSettings.SchemaVersion);
        Assert.True(service.CurrentSettings.CloseToTray);
    }
}
