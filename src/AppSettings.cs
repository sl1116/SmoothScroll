using System.Text.Json.Serialization;

namespace SmoothScrollLocal;

public sealed class AppSettings
{
    public const int DefaultAnimationDurationMs = 72;
    public const int DefaultFrameCount = 9;
    public const int DefaultMinimumFrameDelta = 0;
    public const double DefaultWheelMultiplier = 2.8;

    public const int FastAnimationDurationMs = 48;
    public const int FastFrameCount = 4;
    public const int FastMinimumFrameDelta = 30;
    public const double FastWheelMultiplier = 4.0;

    public bool Enabled { get; set; } = true;

    public int AnimationDurationMs { get; set; } = DefaultAnimationDurationMs;

    public int FrameCount { get; set; } = DefaultFrameCount;

    public int MinimumFrameDelta { get; set; } = DefaultMinimumFrameDelta;

    public double WheelMultiplier { get; set; } = DefaultWheelMultiplier;

    public List<string> DisabledProcessNames { get; set; } = [];

    [JsonIgnore]
    public int FrameDelayMs => Math.Max(1, AnimationDurationMs / Math.Max(1, FrameCount));

    public bool IsProcessDisabled(string? processName)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            return false;
        }

        return DisabledProcessNames.Any(name =>
            string.Equals(name, processName, StringComparison.OrdinalIgnoreCase));
    }

    public void Normalize()
    {
        AnimationDurationMs = Math.Clamp(AnimationDurationMs, 30, 1000);
        FrameCount = Math.Clamp(FrameCount, 1, 60);
        MinimumFrameDelta = Math.Clamp(MinimumFrameDelta, 0, NativeMethods.WHEEL_DELTA);
        WheelMultiplier = Math.Clamp(WheelMultiplier, 0.1, 5.0);

        DisabledProcessNames = DisabledProcessNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public void ApplySmootherPreset()
    {
        AnimationDurationMs = DefaultAnimationDurationMs;
        FrameCount = DefaultFrameCount;
        MinimumFrameDelta = DefaultMinimumFrameDelta;
        WheelMultiplier = DefaultWheelMultiplier;
    }

    public void ApplyFastPreset()
    {
        AnimationDurationMs = FastAnimationDurationMs;
        FrameCount = FastFrameCount;
        MinimumFrameDelta = FastMinimumFrameDelta;
        WheelMultiplier = FastWheelMultiplier;
    }
}
