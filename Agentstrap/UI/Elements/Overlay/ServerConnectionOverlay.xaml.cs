using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Agentstrap.UI.Elements.Overlay
{
    /// <summary>
    /// Borderless, click-through toast window that hovers over the top center of the
    /// Roblox client window. Purely presentational - it renders whatever text it is given
    /// and follows the target window while a notification is showing. All positioning and
    /// lifecycle decisions are delegated to <see cref="ServerConnectionNotifier"/>.
    /// </summary>
    public partial class ServerConnectionOverlay : Window
    {
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;

        /// <summary>
        /// <c>GetWindow</c> command: the window immediately above the given window in the z-order.
        /// </summary>
        private const uint GW_HWNDPREV = 3;

        private const int WS_EX_TOPMOST_FLAG = 0x00000008;

        /// <summary>
        /// How often the overlay re-aligns itself with the Roblox window while showing.
        /// Only runs while a notification is visible, so this is very cheap.
        /// </summary>
        private const int TrackIntervalMs = 250;

        /// <summary>
        /// How far below the top edge of the Roblox client area the notification sits.
        /// </summary>
        private const double TopOffsetDip = 16;

        private readonly DispatcherTimer _timer;

        private TranslateTransform? _translateTransform;

        private Process? _targetProcess;
        private int _targetProcessId = 0;

        /// <summary>
        /// Time (UTC) at which the currently showing notification should fade out.
        /// </summary>
        private DateTime _visibleUntil = DateTime.MinValue;

        private bool _active = false;
        private bool _windowShown = false;
        private bool _fadingOut = false;

        /// <summary>
        /// Whether the overlay window is currently in the topmost band. Mirrors the
        /// target window's own state; starts false to match <c>Topmost="False"</c>.
        /// </summary>
        private bool _overlayTopmost = false;

        /// <summary>
        /// Ensures the placement diagnostics are only written once per notification
        /// instead of on every tracking tick.
        /// </summary>
        private bool _positionLogged = false;

        /// <summary>
        /// Ensures a missing Roblox window is only reported once per notification, so a
        /// notification that can never be placed is obvious in the log.
        /// </summary>
        private bool _targetMissingLogged = false;

        /// <summary>
        /// Test seam: when set, the overlay tracks this window handle directly instead
        /// of resolving it through the target process. Always zero in production.
        /// </summary>
        internal IntPtr TargetWindowOverride = IntPtr.Zero;

        public ServerConnectionOverlay()
        {
            // guarantees that post-Show Topmost changes on this window actually reach
            // the native topmost band (see WarmUpThread)
            WarmUpThread();

            InitializeComponent();

            _translateTransform = BorderTranslate;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(TrackIntervalMs)
            };

            _timer.Tick += Timer_Tick;

            Closed += (_, _) =>
            {
                _active = false;
                _windowShown = false;
                _timer.Stop();
            };
        }

        /// <summary>
        /// Shows (or updates) the notification with the given text, keeping it visible
        /// for the given number of seconds while it tracks the window of the given
        /// Roblox process. Any notification that is already showing is replaced.
        /// </summary>
        public void ShowNotification(string text, int targetProcessId, int durationSeconds)
        {
            const string LOG_IDENT = "ServerConnectionOverlay::ShowNotification";

            App.Logger.WriteLine(LOG_IDENT, $"Showing notification for {durationSeconds} seconds: {text}");

            _targetProcessId = targetProcessId;
            NotificationText.Text = text;

            _visibleUntil = DateTime.UtcNow.AddSeconds(Math.Clamp(durationSeconds, 1, 30));
            _active = true;
            _positionLogged = false;
            _targetMissingLogged = false;

            bool wasFadingOut = _fadingOut;
            _fadingOut = false;

            RefreshTarget();

            // a new notification arrived while the previous one was fading away
            if (wasFadingOut && _windowShown)
                AnimateIn();

            if (!_timer.IsEnabled)
                _timer.Start();
        }

        /// <summary>
        /// Replaces the text of the notification that is currently showing, without
        /// restarting its timer. Used to enrich a notification that was displayed
        /// immediately with details that came back from slower lookups. A notification
        /// that has already faded out is not resurrected.
        /// </summary>
        public void UpdateNotification(string text)
        {
            const string LOG_IDENT = "ServerConnectionOverlay::UpdateNotification";

            if (!_active)
                return;

            App.Logger.WriteLine(LOG_IDENT, $"Updating notification text: {text}");

            NotificationText.Text = text;
        }

        /// <summary>
        /// Fades out and hides the current notification, if any.
        /// </summary>
        public void HideNotification()
        {
            _active = false;
            _timer.Stop();

            FadeOut();
        }

        /// <summary>
        /// Stops tracking and closes the overlay window, cleaning up regardless of
        /// which thread we are being called from.
        /// </summary>
        public void Shutdown()
        {
            _active = false;
            _timer.Stop();

            if (Dispatcher.CheckAccess())
                Close();
            else
                Dispatcher.Invoke(() => Close());
        }

        /// <summary>
        /// Registers the overlay as a tool window (so it stays out of the alt+tab switcher)
        /// and makes it click-through and non-activating, so it can never interfere with
        /// gameplay, mouse input or keyboard focus.
        /// </summary>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // same tool window trick as MenuContainer
            // https://stackoverflow.com/a/551847/11852173
            HWND hWnd = (HWND)new WindowInteropHelper(this).Handle;

            int exStyle = PInvoke.GetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
            exStyle |= 0x00000080 | 0x08000000 | 0x00000020; //WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT
            PInvoke.SetWindowLong(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, exStyle);
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!_active)
            {
                _timer.Stop();
                return;
            }

            if (DateTime.UtcNow >= _visibleUntil)
            {
                FadeOut();
                return;
            }

            RefreshTarget();
        }

        /// <summary>
        /// Shows, repositions or hides the overlay depending on the current state of the
        /// Roblox window. The overlay is placed directly above the Roblox window in the
        /// z-order rather than being made topmost, so it doesn't sit above unrelated
        /// applications.
        /// </summary>
        private void RefreshTarget()
        {
            IntPtr hWnd = GetTargetWindow();

            if (hWnd == IntPtr.Zero || !IsWindow(hWnd) || IsIconic(hWnd))
            {
                // roblox is minimized, gone, or hasn't created its window yet
                if (_windowShown)
                {
                    HideWindow();
                }
                else if (_active && !_targetMissingLogged)
                {
                    // the notification is up but can never be placed - say so, otherwise
                    // this failure mode is completely silent
                    _targetMissingLogged = true;

                    App.Logger.WriteLine("ServerConnectionOverlay::RefreshTarget",
                        $"No usable window found for process {_targetProcessId} (last handle 0x{hWnd.ToInt64():X}) - notification stays hidden");
                }

                return;
            }

            if (!_windowShown)
            {
                ShowWindowOverTarget(hWnd);
                return;
            }

            PositionOverTarget(hWnd);
        }

        private void ShowWindowOverTarget(IntPtr hWnd)
        {
            Opacity = 0;
            Show();

            PositionOverTarget(hWnd);

            _windowShown = true;

            AnimateIn();
        }

        private void HideWindow()
        {
            _windowShown = false;

            BeginAnimation(OpacityProperty, null);
            _translateTransform?.BeginAnimation(TranslateTransform.YProperty, null);

            Hide();
        }

        /// <summary>
        /// Centers the overlay horizontally at the top of the Roblox client area.
        /// Coordinates are calculated in the same coordinate space as the Win32 window
        /// functions we use, so this stays correct across DPI scaled displays.
        /// </summary>
        private void PositionOverTarget(IntPtr hWnd)
        {
            const string LOG_IDENT = "ServerConnectionOverlay::PositionOverTarget";

            if (ActualWidth < 1 || ActualHeight < 1)
                UpdateLayout(); // SizeToContent hasn't been laid out yet

            if (!GetClientRect(hWnd, out NativeRect clientRect))
                return;

            double scale = GetScale(hWnd);

            double widthPx = ActualWidth * scale;

            if (clientRect.Right <= 0 || clientRect.Bottom <= 0 || widthPx <= 0)
                return;

            NativePoint clientOrigin = new(0, 0);

            if (!ClientToScreen(hWnd, ref clientOrigin))
                return;

            int x = clientOrigin.X + (int)Math.Round((clientRect.Right - widthPx) / 2);
            int y = clientOrigin.Y + (int)Math.Round(TopOffsetDip * scale);

            IntPtr overlayHwnd = new WindowInteropHelper(this).Handle;

            if (overlayHwnd == IntPtr.Zero)
                return;

            // A game window that covers its monitor is presented through the fullscreen
            // optimisation / independent flip path, where a window sitting above it in the
            // z-order is simply not composited - the only reliable way to paint over it is to
            // join the topmost band. Windowed and merely maximized targets keep mirroring the
            // target's own band, so the toast never floats over unrelated applications.
            bool coversMonitor = ClientCoversItsMonitor(hWnd, clientOrigin, clientRect.Right, clientRect.Bottom);

            MirrorTopmostState(hWnd, coversMonitor);

            IntPtr insertAfter;
            uint flags = SWP_NOSIZE | SWP_NOACTIVATE;

            if (_overlayTopmost)
            {
                // In the topmost band the exact ordering does not matter - what matters is
                // staying above every non-topmost window, which HWND_TOPMOST guarantees.
                insertAfter = HWND_TOPMOST;
            }
            else
            {
                // Place the overlay immediately above the target window, without activating
                // either window. SetWindowPos inserts the window *after* hWndInsertAfter in the
                // z-order, i.e. behind it - so passing the target itself would tuck the
                // notification under the game (where it is completely invisible). The window
                // currently sitting above the target is passed instead; when the target is
                // already frontmost there is none and HWND_TOP (zero) puts the overlay on top.
                insertAfter = GetWindow(hWnd, GW_HWNDPREV);

                if (insertAfter == overlayHwnd)
                {
                    // The overlay is already sitting directly above the target - which is the
                    // normal case once it is showing. Handing Windows that same handle to insert
                    // behind is a silent no-op (it reports success and moves nothing at all), so
                    // the z-order is left alone and only the position is updated.
                    insertAfter = IntPtr.Zero;
                    flags |= SWP_NOZORDER;
                }
            }

            SetWindowPos(overlayHwnd, insertAfter, x, y, 0, 0, flags);

            if (!_positionLogged)
            {
                _positionLogged = true;

                // The failure mode here is silent (a correctly positioned window that is simply
                // never painted), so walk the z-order and report where the overlay really ended
                // up. The immediate neighbour above the target is not a reliable answer - hidden
                // helper windows can be sandwiched in between.
                int windowsBetween = -1;
                IntPtr walker = hWnd;

                for (int i = 0; i < 50; i++)
                {
                    walker = GetWindow(walker, GW_HWNDPREV);

                    if (walker == IntPtr.Zero)
                        break;

                    if (walker == overlayHwnd)
                    {
                        windowsBetween = i;
                        break;
                    }
                }

                string zOrder = windowsBetween >= 0
                    ? $"above target ({windowsBetween} window(s) between)"
                    : "NOT above target";

                App.Logger.WriteLine(LOG_IDENT, $"Positioned at ({x},{y}) over 0x{hWnd.ToInt64():X} (client {clientRect.Right}x{clientRect.Bottom} at {clientOrigin.X},{clientOrigin.Y}, scale {scale}, topmost {_overlayTopmost}, coversMonitor {coversMonitor}, z-order {zOrder})");
            }
        }

        /// <summary>
        /// WPF silently ignores post-Show <c>Topmost</c> changes on the first window a
        /// thread creates (the property updates, but the native topmost band never
        /// changes) - which would leave the overlay unable to follow a fullscreen
        /// game's band. Creating and discarding a throwaway 1x1 window first costs
        /// about a millisecond and removes that quirk for every window created after.
        /// </summary>
        internal static void WarmUpThread()
        {
            try
            {
                Window warmup = new()
                {
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    Width = 1,
                    Height = 1,
                    Left = -10000,
                    Top = -10000
                };

                warmup.Show();

                // the quirk only clears once the first window has been pumped (i.e.
                // finished its initialization pass), so briefly run the dispatcher
                // before discarding the window again
                DispatcherFrame frame = new();

                DispatcherTimer timer = new(DispatcherPriority.Background)
                {
                    Interval = TimeSpan.FromMilliseconds(150)
                };

                timer.Tick += (_, _) =>
                {
                    frame.Continue = false;
                    timer.Stop();
                };

                timer.Start();
                Dispatcher.PushFrame(frame);

                warmup.Close();
            }
            catch (Exception)
            {
                // the warmup is an optimization - never let it break the overlay
            }
        }

        /// <summary>
        /// Puts the overlay into the same topmost band as the target window (and back
        /// out again when the target isn't topmost), re-running only on state changes.
        /// The WPF Topmost property is used rather than a raw SetWindowPos call so
        /// WPF itself owns the band change and cannot revert it afterwards.
        /// </summary>
        /// <param name="targetHwnd">The window the overlay is tracking.</param>
        /// <param name="forceTopmost">
        /// True when the target covers its monitor and is therefore presented by the
        /// fullscreen optimisation path - such a window paints over anything merely
        /// above it in the z-order, so the toast has to join the topmost band to be seen.
        /// </param>
        private void MirrorTopmostState(IntPtr targetHwnd, bool forceTopmost)
        {
            bool targetTopmost = forceTopmost
                || (PInvoke.GetWindowLong((HWND)targetHwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE) & WS_EX_TOPMOST_FLAG) != 0;

            if (targetTopmost == _overlayTopmost)
                return;

            Topmost = targetTopmost;

            // verify the band change actually took; setting the DP to the same value
            // again would not re-fire WPF's change handling, so a failed application
            // is recovered by toggling the property once
            if (!IsTopmostApplied(targetTopmost))
            {
                Topmost = !targetTopmost;
                Topmost = targetTopmost;
            }

            if (IsTopmostApplied(targetTopmost))
            {
                _overlayTopmost = targetTopmost;
            }
            else
            {
                App.Logger.WriteLine("ServerConnectionOverlay::MirrorTopmostState",
                    $"Topmost change to {targetTopmost} did not take effect");
            }
        }

        /// <summary>
        /// Reads the overlay's native topmost bit back to confirm a band change.
        /// </summary>
        private bool IsTopmostApplied(bool expected)
        {
            IntPtr overlayHwnd = new WindowInteropHelper(this).Handle;

            if (overlayHwnd == IntPtr.Zero)
                return false;

            bool actual = (PInvoke.GetWindowLong((HWND)overlayHwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE) & WS_EX_TOPMOST_FLAG) != 0;

            return actual == expected;
        }

        private IntPtr GetTargetWindow()
        {
            if (TargetWindowOverride != IntPtr.Zero)
                return TargetWindowOverride;

            try
            {
                _targetProcess ??= Process.GetProcessById(_targetProcessId);

                if (_targetProcess.HasExited)
                    return IntPtr.Zero;

                _targetProcess.Refresh();

                // MainWindowHandle can point at a transient splash or popup window while
                // the game is starting up - the game viewport is always the process's
                // largest visible top-level window
                IntPtr largestWindow = FindLargestVisibleWindow(_targetProcess.Id);

                return largestWindow != IntPtr.Zero
                    ? largestWindow
                    : _targetProcess.MainWindowHandle;
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Returns the largest visible top-level window of the given process, or zero
        /// when none can be found. Used to reliably pick the game viewport over any
        /// smaller windows the client may briefly show.
        /// </summary>
        internal static IntPtr FindLargestVisibleWindow(int processId)
        {
            IntPtr best = IntPtr.Zero;
            long bestArea = 0;

            EnumWindows((hWnd, _) =>
            {
                if (!IsWindowVisible(hWnd))
                    return true;

                GetWindowThreadProcessId(hWnd, out uint windowProcessId);

                if (windowProcessId != (uint)processId)
                    return true;

                if (!GetWindowRect(hWnd, out NativeRect rect))
                    return true;

                long area = (long)Math.Max(0, rect.Right - rect.Left) * Math.Max(0, rect.Bottom - rect.Top);

                if (area > bestArea)
                {
                    bestArea = area;
                    best = hWnd;
                }

                return true;
            }, IntPtr.Zero);

            return bestArea > 0 ? best : IntPtr.Zero;
        }

        private void AnimateIn()
        {
            _translateTransform?.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation
            {
                From = -8,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            });

            BeginAnimation(OpacityProperty, new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(150)
            });
        }

        private void FadeOut()
        {
            if (_fadingOut || !_windowShown)
                return;

            _fadingOut = true;

            // the notification is over - stop tracking so we never resurrect it
            _active = false;
            _timer.Stop();

            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(250),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            fadeOut.Completed += (_, _) =>
            {
                // a newer notification may have arrived while we were fading out
                if (_fadingOut)
                    HideWindow();
            };

            BeginAnimation(OpacityProperty, fadeOut);
        }

        /// <summary>
        /// True when the window's client area covers the whole of the monitor it sits on,
        /// i.e. the game is running fullscreen (or borderless fullscreen). Modern Windows
        /// hands such a window to the independent flip presentation path, which is not
        /// composited with windows that merely sit above it in the z-order.
        /// </summary>
        private static bool ClientCoversItsMonitor(IntPtr hWnd, NativePoint clientOrigin, int clientWidth, int clientHeight)
        {
            // a borderless fullscreen client area matches the monitor exactly; the slack
            // absorbs the odd pixel of rounding without ever matching a taskbar-sized inset
            const int tolerance = 8;

            IntPtr monitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);

            if (monitor == IntPtr.Zero)
                return false;

            MonitorInfo info = new() { cbSize = Marshal.SizeOf<MonitorInfo>() };

            if (!GetMonitorInfo(monitor, ref info))
                return false;

            NativeRect bounds = info.rcMonitor;

            return clientOrigin.X <= bounds.Left + tolerance
                && clientOrigin.Y <= bounds.Top + tolerance
                && clientOrigin.X + clientWidth >= bounds.Right - tolerance
                && clientOrigin.Y + clientHeight >= bounds.Bottom - tolerance;
        }

        #region Native methods
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MonitorInfo
        {
            public int cbSize;
            public NativeRect rcMonitor;
            public NativeRect rcWork;
            public uint dwFlags;
        }

        private const uint MONITOR_DEFAULTTONEAREST = 2;

        private static readonly IntPtr HWND_TOPMOST = new(-1);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint dwFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int X;
            public int Y;

            public NativePoint(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out NativeRect lpRect);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr hWnd, out NativeRect lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ClientToScreen(IntPtr hWnd, ref NativePoint lpPoint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hWnd);

        private static double GetScale(IntPtr hWnd)
        {
            try
            {
                uint dpi = GetDpiForWindow(hWnd);

                if (dpi > 0)
                    return dpi / 96.0;
            }
            catch (EntryPointNotFoundException)
            {
                // only reachable on pre-1607 Windows 10, which Agentstrap doesn't support anyway
            }

            return 1.0;
        }
        #endregion
    }
}
