using System.Diagnostics;

namespace SmoothScrollLocal;

public sealed class TrayAppContext : ApplicationContext
{
    private readonly SettingsStore _settingsStore;
    private readonly MouseHook _mouseHook;
    private readonly Icon _appIcon;
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _enabledMenuItem;

    public TrayAppContext(SettingsStore settingsStore, MouseHook mouseHook)
    {
        _settingsStore = settingsStore;
        _mouseHook = mouseHook;

        _enabledMenuItem = new ToolStripMenuItem("Enable SmoothScroll")
        {
            CheckOnClick = true,
            Checked = _settingsStore.Current.Enabled
        };
        _enabledMenuItem.CheckedChanged += (_, _) =>
        {
            _settingsStore.Update(settings => settings.Enabled = _enabledMenuItem.Checked);
            UpdateTooltip();
        };

        var menu = new ContextMenuStrip();
        menu.Items.Add(_enabledMenuItem);
        menu.Items.Add(new ToolStripMenuItem("Use smoother preset", null, (_, _) => ApplySmootherPreset()));
        menu.Items.Add(new ToolStripMenuItem("Use faster preset", null, (_, _) => ApplyFastPreset()));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("Disable for current app", null, (_, _) => DisableCurrentApp()));
        menu.Items.Add(new ToolStripMenuItem("Open config file", null, (_, _) => OpenConfigFile()));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("Exit", null, (_, _) => ExitThread()));

        _appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? (Icon)SystemIcons.Application.Clone();
        _notifyIcon = new NotifyIcon
        {
            Icon = _appIcon,
            ContextMenuStrip = menu,
            Visible = true
        };

        UpdateTooltip();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _appIcon.Dispose();
            _mouseHook.Dispose();
            _settingsStore.Dispose();
        }

        base.Dispose(disposing);
    }

    private void DisableCurrentApp()
    {
        var processName =
            WindowProcessDetector.GetProcessNameUnderCursor() ??
            WindowProcessDetector.GetForegroundProcessName();
        if (string.IsNullOrWhiteSpace(processName))
        {
            MessageBox.Show(
                "Cannot detect the active app right now.",
                "SmoothScroll Local",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        _settingsStore.Update(settings =>
        {
            if (!settings.DisabledProcessNames.Contains(processName, StringComparer.OrdinalIgnoreCase))
            {
                settings.DisabledProcessNames.Add(processName);
            }
        });

        MessageBox.Show(
            $"Disabled SmoothScroll for {processName}.",
            "SmoothScroll Local",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void ApplySmootherPreset()
    {
        _settingsStore.Update(settings => settings.ApplySmootherPreset());
        UpdateTooltip();
        ShowPresetApplied("Smoother", _settingsStore.Current);
    }

    private void ApplyFastPreset()
    {
        _settingsStore.Update(settings => settings.ApplyFastPreset());
        UpdateTooltip();
        ShowPresetApplied("Fast", _settingsStore.Current);
    }

    private static void ShowPresetApplied(string presetName, AppSettings settings)
    {
        MessageBox.Show(
            $"{presetName} preset applied.\n\n" +
            $"Duration: {settings.AnimationDurationMs} ms\n" +
            $"Frames: {settings.FrameCount}\n" +
            $"Minimum delta: {settings.MinimumFrameDelta}\n" +
            $"Speed multiplier: {settings.WheelMultiplier:0.##}",
            "SmoothScroll Local",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void OpenConfigFile()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = _settingsStore.ConfigPath,
            UseShellExecute = true
        };

        try
        {
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Cannot open config file",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void UpdateTooltip()
    {
        var state = _settingsStore.Current.Enabled ? "enabled" : "disabled";
        _notifyIcon.Text = $"SmoothScroll Local ({state})";
    }
}
