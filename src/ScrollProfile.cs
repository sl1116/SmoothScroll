using System.Text.Json.Serialization;

namespace SmoothScrollLocal;

public sealed class ScrollProfile
{
    public const double MaxWheelMultiplier = 12.0;

    public int AnimationDurationMs { get; set; } = ScrollPresets.SmootherAnimationDurationMs;

    public int FrameCount { get; set; } = ScrollPresets.SmootherFrameCount;

    public int MinimumFrameDelta { get; set; } = ScrollPresets.SmootherMinimumFrameDelta;

    public double WheelMultiplier { get; set; } = ScrollPresets.SmootherWheelMultiplier;

    [JsonIgnore]
    public int FrameDelayMs => Math.Max(1, AnimationDurationMs / Math.Max(1, FrameCount));

    public ScrollProfile Clone()
    {
        return new ScrollProfile
        {
            AnimationDurationMs = AnimationDurationMs,
            FrameCount = FrameCount,
            MinimumFrameDelta = MinimumFrameDelta,
            WheelMultiplier = WheelMultiplier
        };
    }

    public void Normalize()
    {
        AnimationDurationMs = Math.Clamp(AnimationDurationMs, 30, 1000);
        FrameCount = Math.Clamp(FrameCount, 1, 60);
        MinimumFrameDelta = Math.Clamp(MinimumFrameDelta, 0, NativeMethods.WHEEL_DELTA);
        WheelMultiplier = Math.Clamp(WheelMultiplier, 0.1, MaxWheelMultiplier);
    }
}
