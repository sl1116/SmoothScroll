using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SmoothScrollLocal;

internal static partial class NativeMethods
{
    public const int WH_MOUSE_LL = 14;
    public const int WM_MOUSEWHEEL = 0x020A;
    public const int WM_MOUSEHWHEEL = 0x020E;
    public const int WHEEL_DELTA = 120;
    public const int LL_MHF_INJECTED = 0x00000001;
    public const uint INPUT_MOUSE = 0;
    public const uint MOUSEEVENTF_WHEEL = 0x0800;
    public const uint MOUSEEVENTF_HWHEEL = 0x01000;

    public delegate nint LowLevelMouseProc(int nCode, nint wParam, nint lParam);

    [LibraryImport("user32.dll", SetLastError = true)]
    public static partial nint SetWindowsHookExW(
        int idHook,
        LowLevelMouseProc lpfn,
        nint hMod,
        uint dwThreadId);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UnhookWindowsHookEx(nint hhk);

    [LibraryImport("user32.dll")]
    public static partial nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

    [LibraryImport("user32.dll")]
    public static partial uint SendInput(uint cInputs, INPUT[] pInputs, int cbSize);

    [LibraryImport("user32.dll")]
    public static partial nint GetForegroundWindow();

    [LibraryImport("user32.dll")]
    public static partial nint WindowFromPoint(POINT point);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetCursorPos(out POINT point);

    [LibraryImport("user32.dll")]
    public static partial uint GetWindowThreadProcessId(nint hWnd, out uint processId);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public POINT Pt;
        public int MouseData;
        public int Flags;
        public int Time;
        public nint DwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public uint Type;
        public MOUSEINPUT MouseInput;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        public int Dx;
        public int Dy;
        public int MouseData;
        public uint DwFlags;
        public uint Time;
        public nint DwExtraInfo;
    }

    public static string? GetForegroundProcessName()
    {
        var foregroundWindow = GetForegroundWindow();
        if (foregroundWindow == nint.Zero)
        {
            return null;
        }

        _ = GetWindowThreadProcessId(foregroundWindow, out var processId);
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

    public static string? GetProcessNameFromPoint(POINT point)
    {
        var window = WindowFromPoint(point);
        return GetProcessNameFromWindow(window);
    }

    public static string? GetProcessNameUnderCursor()
    {
        return GetCursorPos(out var point) ? GetProcessNameFromPoint(point) : null;
    }

    private static string? GetProcessNameFromWindow(nint window)
    {
        if (window == nint.Zero)
        {
            return null;
        }

        _ = GetWindowThreadProcessId(window, out var processId);
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
