using System.Diagnostics;

namespace SmoothScrollLocal;

internal static class WindowProcessDetector
{
    public static string? GetForegroundProcessName()
    {
        return GetProcessNameFromWindow(NativeMethods.GetForegroundWindow());
    }

    public static string? GetProcessNameFromPoint(NativeMethods.POINT point)
    {
        return GetProcessNameFromWindow(NativeMethods.WindowFromPoint(point));
    }

    public static string? GetProcessNameUnderCursor()
    {
        return NativeMethods.GetCursorPos(out var point) ? GetProcessNameFromPoint(point) : null;
    }

    private static string? GetProcessNameFromWindow(nint window)
    {
        if (window == nint.Zero)
        {
            return null;
        }

        _ = NativeMethods.GetWindowThreadProcessId(window, out var processId);
        if (processId == 0)
        {
            return null;
        }

        try
        {
            return Process.GetProcessById((int)processId).ProcessName;
        }
        catch
        {
            return null;
        }
    }
}
