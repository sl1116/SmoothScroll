using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace SmoothScrollLocal;

public sealed class SmoothScrollAnimator : IDisposable
{
    private readonly BlockingCollection<WheelRequest> _queue = new();
    private readonly CancellationTokenSource _shutdown = new();
    private readonly SettingsStore _settingsStore;
    private readonly Task _worker;

    public SmoothScrollAnimator(SettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
        _worker = Task.Run(ProcessQueue);
    }

    public void EnqueueWheel(int delta, bool horizontal)
    {
        if (!_queue.IsAddingCompleted)
        {
            _queue.Add(new WheelRequest(delta, horizontal));
        }
    }

    public void Dispose()
    {
        _queue.CompleteAdding();
        _shutdown.Cancel();

        try
        {
            _worker.Wait(300);
        }
        catch
        {
            // App is exiting; do not block shutdown on the animation worker.
        }

        _queue.Dispose();
        _shutdown.Dispose();
    }

    private void ProcessQueue()
    {
        while (!_shutdown.IsCancellationRequested)
        {
            WheelRequest request;
            try
            {
                request = _queue.Take(_shutdown.Token);
            }
            catch
            {
                return;
            }

            var totalDelta = request.Delta;
            while (_queue.TryTake(out var next) && next.Horizontal == request.Horizontal)
            {
                totalDelta += next.Delta;
            }

            Animate(totalDelta, request.Horizontal);
        }
    }

    private void Animate(int rawDelta, bool horizontal)
    {
        var settings = _settingsStore.Current;
        var frameCount = Math.Max(1, settings.FrameCount);
        var targetDelta = (int)Math.Round(rawDelta * settings.WheelMultiplier);
        var sentDelta = 0;

        for (var frame = 1; frame <= frameCount && !_shutdown.IsCancellationRequested; frame++)
        {
            var progress = frame / (double)frameCount;
            var easedProgress = EaseOutCubic(progress);
            var desiredTotal = (int)Math.Round(targetDelta * easedProgress);
            var frameDelta = desiredTotal - sentDelta;

            if (frameDelta != 0)
            {
                SendWheel(frameDelta, horizontal);
                sentDelta += frameDelta;
            }

            Thread.Sleep(settings.FrameDelayMs);
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

    private readonly record struct WheelRequest(int Delta, bool Horizontal);
}
