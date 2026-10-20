namespace AndDesk.Services;

public interface IStartupService
{
    bool IsStartupEnabled();
    bool SetStartup(bool enable);
}
