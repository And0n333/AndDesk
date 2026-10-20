using System.IO;
using System.Text.Json;
using AndDesk.Models;

namespace AndDesk.Services;

public class SettingsService : ISettingsService
{
    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AndDesk"
    );

    private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "settings.json");

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public AppSettings CurrentSettings { get; private set; } = new();

    public SettingsService()
    {
        LoadSettings();
    }

    public void LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
                if (loaded != null)
                {
                    CurrentSettings = loaded;
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            // If corrupt, backup corrupt file and fall back to defaults
            try
            {
                string backupPath = SettingsFilePath + ".corrupt." + DateTime.Now.ToString("yyyyMMddHHmmss");
                if (File.Exists(SettingsFilePath))
                {
                    File.Copy(SettingsFilePath, backupPath, true);
                }
            }
            catch
            {
                // Ignore backup failure
            }
            System.Diagnostics.Debug.WriteLine($"Failed to load settings, reverted to default: {ex.Message}");
        }

        CurrentSettings = new AppSettings();
        SaveSettings();
    }

    public void SaveSettings()
    {
        try
        {
            if (!Directory.Exists(SettingsDirectory))
            {
                Directory.CreateDirectory(SettingsDirectory);
            }

            string tempFilePath = Path.Combine(SettingsDirectory, "settings.json.tmp");
            string json = JsonSerializer.Serialize(CurrentSettings, _jsonOptions);
            File.WriteAllText(tempFilePath, json);

            if (File.Exists(SettingsFilePath))
            {
                File.Replace(tempFilePath, SettingsFilePath, null);
            }
            else
            {
                File.Move(tempFilePath, SettingsFilePath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
        }
    }

    public void UpdateSettings(Action<AppSettings> updateAction)
    {
        updateAction(CurrentSettings);
        SaveSettings();
    }
}
