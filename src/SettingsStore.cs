using System.Text.Json;

namespace SmoothScrollLocal;

public sealed class SettingsStore : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly object _gate = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private AppSettings _settings;

    public SettingsStore()
    {
        Directory.CreateDirectory(ConfigDirectory);
        _settings = LoadFromDisk();

        _watcher = new FileSystemWatcher(ConfigDirectory, Path.GetFileName(ConfigPath))
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName
        };
        _watcher.Changed += (_, _) => ReloadQuietly();
        _watcher.Created += (_, _) => ReloadQuietly();
        _watcher.EnableRaisingEvents = true;

        Save();
    }

    public string ConfigDirectory { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SmoothScrollLocal");

    public string ConfigPath => Path.Combine(ConfigDirectory, "config.json");

    public AppSettings Current
    {
        get
        {
            lock (_gate)
            {
                return new AppSettings
                {
                    Enabled = _settings.Enabled,
                    AnimationDurationMs = _settings.AnimationDurationMs,
                    FrameCount = _settings.FrameCount,
                    WheelMultiplier = _settings.WheelMultiplier,
                    DisabledProcessNames = [.. _settings.DisabledProcessNames]
                };
            }
        }
    }

    public void Update(Action<AppSettings> update)
    {
        lock (_gate)
        {
            update(_settings);
            _settings.Normalize();
            SaveLocked();
        }
    }

    public void Save()
    {
        lock (_gate)
        {
            SaveLocked();
        }
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }

    private AppSettings LoadFromDisk()
    {
        try
        {
            if (!File.Exists(ConfigPath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(ConfigPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
            settings.Normalize();
            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }

    private void ReloadQuietly()
    {
        try
        {
            Thread.Sleep(50);
            lock (_gate)
            {
                _settings = LoadFromDisk();
            }
        }
        catch
        {
            // Keep the last good config if the file is being edited.
        }
    }

    private void SaveLocked()
    {
        _watcher.EnableRaisingEvents = false;
        try
        {
            var json = JsonSerializer.Serialize(_settings, _jsonOptions);
            File.WriteAllText(ConfigPath, json);
        }
        finally
        {
            _watcher.EnableRaisingEvents = true;
        }
    }
}
