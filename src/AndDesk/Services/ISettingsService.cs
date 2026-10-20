using AndDesk.Models;

namespace AndDesk.Services;

public interface ISettingsService
{
    AppSettings CurrentSettings { get; }
    void LoadSettings();
    void SaveSettings();
    void UpdateSettings(Action<AppSettings> updateAction);
}
