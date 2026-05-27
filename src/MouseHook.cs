using System.Runtime.InteropServices;

namespace SmoothScrollLocal;

public sealed class MouseHook : IDisposable
{
    private readonly SettingsStore _settingsStore;
    private readonly SmoothScrollAnimator _animator;
    private readonly NativeMethods.LowLevelMouseProc _hookProc;
    private nint _hookHandle;

    public MouseHook(SettingsStore settingsStore, SmoothScrollAnimator animator)
    {
        _settingsStore = settingsStore;
        _animator = animator;
        _hookProc = OnMouseEvent;
    }

    public bool IsRunning => _hookHandle != nint.Zero;

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        _hookHandle = NativeMethods.SetWindowsHookExW(
            NativeMethods.WH_MOUSE_LL,
            _hookProc,
            nint.Zero,
            0);

        if (_hookHandle == nint.Zero)
        {
            var error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Cannot install mouse hook. Win32 error: {error}");
        }
    }

    public void Stop()
    {
        if (!IsRunning)
        {
            return;
        }

        NativeMethods.UnhookWindowsHookEx(_hookHandle);
        _hookHandle = nint.Zero;
    }

    public void Dispose()
    {
        Stop();
    }

    private nint OnMouseEvent(int nCode, nint wParam, nint lParam)
    {
        if (nCode < 0)
        {
            return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        var message = wParam.ToInt32();
        if (message is not NativeMethods.WM_MOUSEWHEEL and not NativeMethods.WM_MOUSEHWHEEL)
        {
            return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        var hookInfo = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
        if ((hookInfo.Flags & NativeMethods.LL_MHF_INJECTED) != 0)
        {
            return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        var settings = _settingsStore.Current;
        var processName = NativeMethods.GetForegroundProcessName();
        if (!settings.Enabled || settings.IsProcessDisabled(processName))
        {
            return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        var wheelDelta = GetWheelDelta(hookInfo.MouseData);
        if (wheelDelta == 0)
        {
            return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        _animator.EnqueueWheel(wheelDelta, horizontal: message == NativeMethods.WM_MOUSEHWHEEL);

        return 1;
    }

    private static int GetWheelDelta(int mouseData)
    {
        return unchecked((short)((mouseData >> 16) & 0xffff));
    }
}
