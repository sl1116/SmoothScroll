using System.Runtime.InteropServices;

namespace SmoothScrollLocal;

public sealed class SmoothScrollAnimator : IDisposable
{
    private readonly AutoResetEvent _hasWork = new(false);
    private readonly CancellationTokenSource _shutdown = new();
    private readonly object _gate = new();
    private readonly SettingsStore _settingsStore;
    private readonly Task _worker;
    private int _pendingVerticalDelta;
    private int _pendingHorizontalDelta;
    private ScrollProfile _pendingVerticalProfile = new();
    private ScrollProfile _pendingHorizontalProfile = new();

    public SmoothScrollAnimator(SettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
        _worker = Task.Run(ProcessQueue);
    }

    public void EnqueueWheel(int delta, bool horizontal, ScrollProfile profile)
    {
        lock (_gate)
        {
            if (_shutdown.IsCancellationRequested)
            {
                return;
            }

            if (horizontal)
            {
                _pendingHorizontalDelta += delta;
                _pendingHorizontalProfile = profile;
            }
            else
            {
                _pendingVerticalDelta += delta;
                _pendingVerticalProfile = profile;
            }
        }

        _hasWork.Set();
    }

    public void Dispose()
    {
        _shutdown.Cancel();
        _hasWork.Set();

        try
        {
            _worker.Wait(300);
        }
        catch
        {
            // App is exiting; do not block shutdown on the animation worker.
        }

        _hasWork.Dispose();
        _shutdown.Dispose();
    }

    private void ProcessQueue()
    {
        while (!_shutdown.IsCancellationRequested)
        {
            if (!TryTakeNext(out var delta, out var horizontal, out var profile))
            {
                _hasWork.WaitOne();
                continue;
            }

            Animate(delta, horizontal, profile);
        }
    }

    private void Animate(int rawDelta, bool horizontal, ScrollProfile profile)
    {
        profile.Normalize();

        var frameCount = Math.Max(1, profile.FrameCount);
        var minimumFrameDelta = Math.Max(0, profile.MinimumFrameDelta);
        var targetDelta = (int)Math.Round(rawDelta * profile.WheelMultiplier);
        var sentDelta = 0;

        for (var frame = 1; frame <= frameCount && !_shutdown.IsCancellationRequested; frame++)
        {
            var newDelta = TakePendingDelta(horizontal, out var pendingProfile);
            if (newDelta != 0)
            {
                pendingProfile.Normalize();
                targetDelta += (int)Math.Round(newDelta * pendingProfile.WheelMultiplier);
            }

            var progress = frame / (double)frameCount;
            var easedProgress = EaseOutCubic(progress);
            var desiredTotal = (int)Math.Round(targetDelta * easedProgress);
            var frameDelta = desiredTotal - sentDelta;
            var isLastFrame = frame == frameCount;

            if (frameDelta != 0 && (isLastFrame || Math.Abs(frameDelta) >= minimumFrameDelta))
            {
                SendWheel(frameDelta, horizontal);
                sentDelta += frameDelta;
            }

            Thread.Sleep(profile.FrameDelayMs);
        }

        var finalDelta = TakePendingDelta(horizontal, out var finalProfile);
        if (finalDelta != 0)
        {
            finalProfile.Normalize();
            targetDelta += (int)Math.Round(finalDelta * finalProfile.WheelMultiplier);
        }

        var remainder = targetDelta - sentDelta;
        if (remainder != 0)
        {
            SendWheel(remainder, horizontal);
        }
    }

    private static double EaseOutCubic(double t)
    {
        var p = 1 - t;
        return 1 - p * p * p;
    }

    private static void SendWheel(int delta, bool horizontal)
    {
        var input = new NativeMethods.INPUT
        {
            Type = NativeMethods.INPUT_MOUSE,
            MouseInput = new NativeMethods.MOUSEINPUT
            {
                MouseData = delta,
                DwFlags = horizontal ? NativeMethods.MOUSEEVENTF_HWHEEL : NativeMethods.MOUSEEVENTF_WHEEL
            }
        };

        var inputs = new[] { input };
        _ = NativeMethods.SendInput(1, inputs, Marshal.SizeOf<NativeMethods.INPUT>());
    }

    private bool TryTakeNext(out int delta, out bool horizontal, out ScrollProfile profile)
    {
        lock (_gate)
        {
            if (_pendingVerticalDelta != 0)
            {
                delta = _pendingVerticalDelta;
                horizontal = false;
                profile = _pendingVerticalProfile.Clone();
                _pendingVerticalDelta = 0;
                return true;
            }

            if (_pendingHorizontalDelta != 0)
            {
                delta = _pendingHorizontalDelta;
                horizontal = true;
                profile = _pendingHorizontalProfile.Clone();
                _pendingHorizontalDelta = 0;
                return true;
            }
        }

        delta = 0;
        horizontal = false;
        profile = new ScrollProfile();
        return false;
    }

    private int TakePendingDelta(bool horizontal, out ScrollProfile profile)
    {
        lock (_gate)
        {
            if (horizontal)
            {
                var delta = _pendingHorizontalDelta;
                profile = _pendingHorizontalProfile.Clone();
                _pendingHorizontalDelta = 0;
                return delta;
            }

            var verticalDelta = _pendingVerticalDelta;
            profile = _pendingVerticalProfile.Clone();
            _pendingVerticalDelta = 0;
            return verticalDelta;
        }
    }
}
