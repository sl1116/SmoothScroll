using System.Text.Json.Serialization;

namespace SmoothScrollLocal;

public sealed class AppSettings
{
    public bool Enabled { get; set; } = true;

    public int AnimationDurationMs { get; set; } = ScrollPresets.SmootherAnimationDurationMs;

    public int FrameCount { get; set; } = ScrollPresets.SmootherFrameCount;

    public int MinimumFrameDelta { get; set; } = ScrollPresets.SmootherMinimumFrameDelta;

    public double WheelMultiplier { get; set; } = ScrollPresets.SmootherWheelMultiplier;

    public List<string> DisabledProcessNames { get; set; } = [];

    public Dictionary<string, ScrollProfile> AppProfiles { get; set; } = new(StringComparer.OrdinalIgnoreCase);

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
        WheelMultiplier = Math.Clamp(WheelMultiplier, 0.1, ScrollProfile.MaxWheelMultiplier);

        AppProfiles = AppProfiles
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key))
            .ToDictionary(
                pair => pair.Key.Trim(),
                pair =>
                {
                    pair.Value.Normalize();
                    return pair.Value;
                },
                StringComparer.OrdinalIgnoreCase);

        DisabledProcessNames = DisabledProcessNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public void ApplySmootherPreset()
    {
        ApplyProfile(ScrollPresets.CreateSmoother());
    }

    public void ApplyFastPreset()
    {
        ApplyProfile(ScrollPresets.CreateFast());
    }

    public ScrollProfile ToScrollProfile()
    {
        return new ScrollProfile
        {
            AnimationDurationMs = AnimationDurationMs,
            FrameCount = FrameCount,
            MinimumFrameDelta = MinimumFrameDelta,
            WheelMultiplier = WheelMultiplier
        };
    }

    public void ApplyProfile(ScrollProfile profile)
    {
        profile.Normalize();

        AnimationDurationMs = profile.AnimationDurationMs;
        FrameCount = profile.FrameCount;
        MinimumFrameDelta = profile.MinimumFrameDelta;
        WheelMultiplier = profile.WheelMultiplier;
    }
}
