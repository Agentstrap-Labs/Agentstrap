using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace Agentstrap.Utility
{
    internal static class RobloxBorderlessWindowService
    {
        private const int GWL_STYLE = -16, GWL_EXSTYLE = -20;
        private const int WH_KEYBOARD_LL = 13;
        private const int HC_ACTION = 0;
        private const int VK_F11 = 0x7A;
        private const int WM_KEYDOWN = 0x0100, WM_KEYUP = 0x0101, WM_SYSKEYDOWN = 0x0104, WM_SYSKEYUP = 0x0105;
        private const long WS_CAPTION = 0x00C00000L, WS_THICKFRAME = 0x00040000L;
        private const long WS_EX_DLGMODALFRAME = 0x1, WS_EX_CLIENTEDGE = 0x200, WS_EX_WINDOWEDGE = 0x100, WS_EX_STATICEDGE = 0x20000;
        private const uint SWP_NOSIZE = 0x1, SWP_NOMOVE = 0x2, SWP_NOZORDER = 0x4, SWP_NOACTIVATE = 0x10, SWP_FRAMECHANGED = 0x20;
        private sealed record OriginalWindow(long Style, long ExtendedStyle, RECT Bounds, int ProcessId, RECT? CenteredBounds);
        private static readonly Dictionary<IntPtr, OriginalWindow> OriginalWindows = new();
        private static readonly DispatcherTimer PollTimer = new() { Interval = TimeSpan.FromMilliseconds(1200) };
        private static readonly LowLevelKeyboardProc KeyboardHookCallbackRoot = KeyboardHookCallback;
        private static bool _started;
        private static IntPtr _keyboardHook;

        public static event EventHandler? StatusChanged;
        public static string Status { get; private set; } = "Waiting for Roblox Player";
        public static IntPtr CurrentWindow { get; private set; }

        static RobloxBorderlessWindowService() => PollTimer.Tick += (_, _) => Update();

        public static void Start()
        {
            if (_started) return;
            if (!IsEnabled()) { SetStatus("Normal window mode"); return; }
            _started = true;
            InstallKeyboardHook();
            if (Application.Current is not null) PollTimer.Start();
            Update();
        }

        public static void Refresh()
        {
            if (IsEnabled()) Start();
            else
            {
                PollTimer.Stop();
                _started = false;
                RemoveKeyboardHook();
                Update();
            }
        }

        private static bool IsEnabled() => App.Settings.Prop.RobloxBorderlessWindowEnabled || App.Settings.Prop.RobloxAutomaticBorderlessModeEnabled;

        public static void RestoreAndDisable()
        {
            App.Settings.Prop.RobloxBorderlessWindowEnabled = false;
            App.Settings.Prop.RobloxAutomaticBorderlessModeEnabled = false;
            App.Settings.Save();
            PollTimer.Stop();
            _started = false;
            RemoveKeyboardHook();
            RestoreTrackedWindows();
            Update();
        }

        private static void Update()
        {
            try
            {
                bool wanted = App.Settings.Prop.RobloxBorderlessWindowEnabled || App.Settings.Prop.RobloxAutomaticBorderlessModeEnabled;
                List<(IntPtr Handle, int ProcessId)> windows = FindPlayerWindows();
                foreach (IntPtr tracked in OriginalWindows.Keys.ToArray())
                {
                    OriginalWindow original = OriginalWindows[tracked];
                    if (!IsWindowOwnedByPlayer(tracked, original.ProcessId))
                    {
                        OriginalWindows.Remove(tracked);
                        if (CurrentWindow == tracked) CurrentWindow = IntPtr.Zero;
                    }
                    else if (!wanted) Restore(tracked, original);
                }

                if (!wanted) { SetStatus("Normal window mode"); return; }
                if (windows.Count == 0) { CurrentWindow = IntPtr.Zero; SetStatus("Waiting for Roblox Player"); return; }
                bool fullscreenDetected = false;
                foreach ((IntPtr handle, int processId) in windows)
                {
                    if (!OriginalWindows.ContainsKey(handle) && IsFullscreenWindow(handle))
                    {
                        fullscreenDetected = true;
                        continue;
                    }
                    if (!OriginalWindows.ContainsKey(handle) && !Apply(handle, processId))
                    { SetStatus("Failed to apply window style"); return; }
                    CurrentWindow = handle;
                }
                SetStatus(CurrentWindow != IntPtr.Zero ? "Borderless mode active" : fullscreenDetected ? "Fullscreen window detected" : "Waiting for Roblox Player");
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("RobloxBorderlessWindow", "Failed while checking the Roblox Player window");
                App.Logger.WriteException("RobloxBorderlessWindow", ex);
                SetStatus("Failed to apply window style");
            }
        }

        private static bool Apply(IntPtr handle, int processId)
        {
            if (!IsWindow(handle) || !GetWindowRect(handle, out RECT bounds)) return false;
            long style = GetWindowLongPtr(handle, GWL_STYLE).ToInt64();
            long extendedStyle = GetWindowLongPtr(handle, GWL_EXSTYLE).ToInt64();
            OriginalWindow original = new(style, extendedStyle, bounds, processId, null);
            OriginalWindows[handle] = original;
            long newStyle = style & ~(WS_CAPTION | WS_THICKFRAME);
            long newExtendedStyle = extendedStyle & ~(WS_EX_DLGMODALFRAME | WS_EX_CLIENTEDGE | WS_EX_STATICEDGE | WS_EX_WINDOWEDGE);
            if (newStyle != style && !TrySetWindowLongPtr(handle, GWL_STYLE, new IntPtr(newStyle))) return FailApply(handle);
            if (newExtendedStyle != extendedStyle && !TrySetWindowLongPtr(handle, GWL_EXSTYLE, new IntPtr(newExtendedStyle))) return FailApply(handle);
            if (App.Settings.Prop.RobloxBorderlessCenterWindow && TryGetCenteredBounds(handle, bounds, out RECT centered))
            {
                OriginalWindows[handle] = original with { CenteredBounds = centered };
                if (!SetWindowPos(handle, IntPtr.Zero, centered.Left, centered.Top, centered.Right - centered.Left, centered.Bottom - centered.Top, SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED)) return FailApply(handle);
            }
            else if (!SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED)) return FailApply(handle);
            return true;
        }

        private static bool FailApply(IntPtr handle)
        {
            if (OriginalWindows.TryGetValue(handle, out OriginalWindow? original)) Restore(handle, original);
            return false;
        }

        private static void RestoreTrackedWindows()
        {
            foreach ((IntPtr handle, OriginalWindow original) in OriginalWindows.ToArray()) Restore(handle, original);
        }

        private static void Restore(IntPtr handle, OriginalWindow original)
        {
            if (!IsWindow(handle)) { OriginalWindows.Remove(handle); return; }
            long style = GetWindowLongPtr(handle, GWL_STYLE).ToInt64();
            long extendedStyle = GetWindowLongPtr(handle, GWL_EXSTYLE).ToInt64();
            long restoredStyle = (style & ~(WS_CAPTION | WS_THICKFRAME)) | (original.Style & (WS_CAPTION | WS_THICKFRAME));
            long extendedMask = WS_EX_DLGMODALFRAME | WS_EX_CLIENTEDGE | WS_EX_STATICEDGE | WS_EX_WINDOWEDGE;
            long restoredExtendedStyle = (extendedStyle & ~extendedMask) | (original.ExtendedStyle & extendedMask);
            bool success = TrySetWindowLongPtr(handle, GWL_STYLE, new IntPtr(restoredStyle)) && TrySetWindowLongPtr(handle, GWL_EXSTYLE, new IntPtr(restoredExtendedStyle));
            if (original.CenteredBounds is RECT centered && GetWindowRect(handle, out RECT current) && SameBounds(current, centered))
                success = SetWindowPos(handle, IntPtr.Zero, original.Bounds.Left, original.Bounds.Top, original.Bounds.Right - original.Bounds.Left, original.Bounds.Bottom - original.Bounds.Top, SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED) && success;
            else
                success = SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED) && success;
            if (!success) SetStatus("Failed to apply window style");
            OriginalWindows.Remove(handle);
            if (CurrentWindow == handle) CurrentWindow = IntPtr.Zero;
        }

        private static bool TryGetCenteredBounds(IntPtr handle, RECT bounds, out RECT centered)
        {
            IntPtr monitor = MonitorFromWindow(handle, 2);
            MONITORINFO info = new() { Size = Marshal.SizeOf<MONITORINFO>() };
            if (monitor != IntPtr.Zero && GetMonitorInfo(monitor, ref info))
            {
                int width = bounds.Right - bounds.Left, height = bounds.Bottom - bounds.Top;
                int x = info.Work.Left + (info.Work.Right - info.Work.Left - width) / 2;
                int y = info.Work.Top + (info.Work.Bottom - info.Work.Top - height) / 2;
                centered = new RECT { Left = x, Top = y, Right = x + width, Bottom = y + height };
                return true;
            }
            centered = default; return false;
        }

        private static bool SameBounds(RECT a, RECT b) => a.Left == b.Left && a.Top == b.Top && a.Right == b.Right && a.Bottom == b.Bottom;

        private static bool IsFullscreenWindow(IntPtr handle)
        {
            if (!GetWindowRect(handle, out RECT bounds)) return false;
            IntPtr monitor = MonitorFromWindow(handle, 2);
            MONITORINFO info = new() { Size = Marshal.SizeOf<MONITORINFO>() };
            if (monitor == IntPtr.Zero || !GetMonitorInfo(monitor, ref info)) return false;
            const int tolerance = 2;
            return Math.Abs(bounds.Left - info.Monitor.Left) <= tolerance
                && Math.Abs(bounds.Top - info.Monitor.Top) <= tolerance
                && Math.Abs(bounds.Right - info.Monitor.Right) <= tolerance
                && Math.Abs(bounds.Bottom - info.Monitor.Bottom) <= tolerance;
        }

        private static bool IsWindowOwnedByPlayer(IntPtr handle, int expectedProcessId)
        {
            if (!IsWindow(handle)) return false;
            GetWindowThreadProcessId(handle, out uint processId);
            if (processId != expectedProcessId) return false;
            try
            {
                using Process process = Process.GetProcessById(expectedProcessId);
                return process.ProcessName.Equals(App.RobloxPlayerAppName, StringComparison.OrdinalIgnoreCase);
            }
            catch (ArgumentException) { return false; }
            catch (InvalidOperationException) { return false; }
        }

        private static List<(IntPtr Handle, int ProcessId)> FindPlayerWindows()
        {
            var result = new List<(IntPtr, int)>();
            EnumWindows((handle, lParam) =>
            {
                if (!IsWindowVisible(handle) || GetWindow(handle, 4) != IntPtr.Zero) return true;
                GetWindowThreadProcessId(handle, out uint processId);
                try
                {
                    using Process process = Process.GetProcessById((int)processId);
                    if (process.ProcessName.Equals(App.RobloxPlayerAppName, StringComparison.OrdinalIgnoreCase)
                        && process.MainWindowHandle == handle
                        && GetWindowRect(handle, out RECT windowBounds))
                        result.Add((handle, (int)processId));
                }
                catch (ArgumentException) { }
                catch (InvalidOperationException) { }
                return true;
            }, IntPtr.Zero);
            return result;
        }

        private static void SetStatus(string status)
        {
            if (Status == status) return;
            Status = status;
            StatusChanged?.Invoke(null, EventArgs.Empty);
        }

        private static void InstallKeyboardHook()
        {
            if (_keyboardHook != IntPtr.Zero) return;
            IntPtr module = GetModuleHandle(null);
            _keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookCallbackRoot, module, 0);
            if (_keyboardHook == IntPtr.Zero)
                App.Logger.WriteLine("RobloxBorderlessWindow", $"Could not install the F11 keyboard filter (Win32 error {Marshal.GetLastWin32Error()})");
        }

        private static void RemoveKeyboardHook()
        {
            if (_keyboardHook == IntPtr.Zero) return;
            UnhookWindowsHookEx(_keyboardHook);
            _keyboardHook = IntPtr.Zero;
        }

        private static IntPtr KeyboardHookCallback(int code, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (code == HC_ACTION && IsF11Message(wParam, lParam) && IsEnabled() && IsRobloxPlayerForeground())
                    return new IntPtr(1);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("RobloxBorderlessWindow", ex);
            }
            return CallNextHookEx(_keyboardHook, code, wParam, lParam);
        }

        private static bool IsF11Message(IntPtr message, IntPtr data)
        {
            int id = message.ToInt32();
            if (id is not (WM_KEYDOWN or WM_KEYUP or WM_SYSKEYDOWN or WM_SYSKEYUP)) return false;
            return data != IntPtr.Zero && Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(data).VirtualKey == VK_F11;
        }

        private static bool IsRobloxPlayerForeground()
        {
            IntPtr foreground = GetForegroundWindow();
            if (foreground == IntPtr.Zero) return false;
            GetWindowThreadProcessId(foreground, out uint processId);
            try
            {
                using Process process = Process.GetProcessById((int)processId);
                return process.ProcessName.Equals(App.RobloxPlayerAppName, StringComparison.OrdinalIgnoreCase);
            }
            catch (ArgumentException) { return false; }
            catch (InvalidOperationException) { return false; }
        }


        [StructLayout(LayoutKind.Sequential)] private struct RECT { public int Left, Top, Right, Bottom; }
        [StructLayout(LayoutKind.Sequential)] private struct MONITORINFO { public int Size; public RECT Monitor; public RECT Work; public uint Flags; }
        [StructLayout(LayoutKind.Sequential)] private struct KBDLLHOOKSTRUCT { public uint VirtualKey, ScanCode, Flags, Time; public UIntPtr ExtraInfo; }
        private delegate bool EnumWindowsCallback(IntPtr hwnd, IntPtr lParam);
        private delegate IntPtr LowLevelKeyboardProc(int code, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsCallback callback, IntPtr lParam);
        [DllImport("user32.dll")] private static extern bool IsWindow(IntPtr hwnd);
        [DllImport("user32.dll")] private static extern bool IsWindowVisible(IntPtr hwnd);
        [DllImport("user32.dll")] private static extern IntPtr GetWindow(IntPtr hwnd, uint command);
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint processId);
        [DllImport("user32.dll", SetLastError = true)] private static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)] private static extern IntPtr GetWindowLongPtr64(IntPtr hwnd, int index);
        [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)] private static extern IntPtr GetWindowLongPtr32(IntPtr hwnd, int index);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)] private static extern IntPtr SetWindowLongPtr64(IntPtr hwnd, int index, IntPtr value);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)] private static extern IntPtr SetWindowLongPtr32(IntPtr hwnd, int index, IntPtr value);
        [DllImport("user32.dll", SetLastError = true)] private static extern bool SetWindowPos(IntPtr hwnd, IntPtr insertAfter, int x, int y, int width, int height, uint flags);
        [DllImport("user32.dll")] private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);
        [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW", CharSet = CharSet.Unicode)] private static extern bool GetMonitorInfo(IntPtr monitor, ref MONITORINFO info);
        [DllImport("user32.dll", SetLastError = true)] private static extern IntPtr SetWindowsHookEx(int hookType, LowLevelKeyboardProc callback, IntPtr module, uint threadId);
        [DllImport("user32.dll", SetLastError = true)] private static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll")] private static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr GetModuleHandle(string? moduleName);
        private static IntPtr GetWindowLongPtr(IntPtr hwnd, int index) => IntPtr.Size == 8 ? GetWindowLongPtr64(hwnd, index) : GetWindowLongPtr32(hwnd, index);
        private static IntPtr SetWindowLongPtr(IntPtr hwnd, int index, IntPtr value) => IntPtr.Size == 8 ? SetWindowLongPtr64(hwnd, index, value) : SetWindowLongPtr32(hwnd, index, value);
        private static bool TrySetWindowLongPtr(IntPtr hwnd, int index, IntPtr value)
        {
            Marshal.SetLastPInvokeError(0);
            IntPtr previous = SetWindowLongPtr(hwnd, index, value);
            return previous != IntPtr.Zero || Marshal.GetLastPInvokeError() == 0;
        }
    }
}
