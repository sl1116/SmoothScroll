namespace SmoothScrollLocal;

public static class ScrollPresets
{
    public const int SmootherAnimationDurationMs = 72;
    public const int SmootherFrameCount = 9;
    public const int SmootherMinimumFrameDelta = 0;
    public const double SmootherWheelMultiplier = 2.8;

    public const int FastAnimationDurationMs = 48;
    public const int FastFrameCount = 4;
    public const int FastMinimumFrameDelta = 30;
    public const double FastWheelMultiplier = 4.0;

    public static ScrollProfile CreateSmoother()
    {
        return new ScrollProfile
        {
            AnimationDurationMs = SmootherAnimationDurationMs,
            FrameCount = SmootherFrameCount,
            MinimumFrameDelta = SmootherMinimumFrameDelta,
            WheelMultiplier = SmootherWheelMultiplier
        };
    }

    public static ScrollProfile CreateFast()
    {
        return new ScrollProfile
        {
            AnimationDurationMs = FastAnimationDurationMs,
            FrameCount = FastFrameCount,
            MinimumFrameDelta = FastMinimumFrameDelta,
            WheelMultiplier = FastWheelMultiplier
        };
    }
}
