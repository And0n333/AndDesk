namespace AndDesk.Services;

public interface IDesktopWatcherService : IDisposable
{
    event Action? OnDesktopChanged;
    void Start();
    void Stop();
}
