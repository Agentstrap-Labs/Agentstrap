using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

using Agentstrap.UI.Elements.Overlay;

using Xunit;

namespace Agentstrap.Tests
{
    /// <summary>
    /// Window-behaviour tests for the server connection overlay: placement at the top
    /// center of the target window, tracking of moves/resizes, minimize handling,
    /// auto-hide after the configured duration, topmost band mirroring, and target
    /// window selection. Everything runs against synthetic WPF windows on an STA
    /// harness thread - no running Roblox client is required.
    /// </summary>
    public class ServerConnectionOverlayTests
    {
        /// <summary>
        /// Must mirror <c>ServerConnectionOverlay.TopOffsetDip</c>.
        /// </summary>
        private const int TopOffsetDip = 16;

        /// <summary>
        /// Allowed pixel deviation between expected and actual placement; absorbs
        /// rounding from DPI scale factors and WPF layout.
        /// </summary>
        private const int TolerancePx = 5;

        static ServerConnectionOverlayTests()
        {
            // the overlay's XAML loads pack://application:,,,/ resources, which resolve
            // against Application.ResourceAssembly - in the real app that is Agentstrap;
            // under the test host it may already be pinned, so never fail here
            try
            {
                Application.ResourceAssembly = typeof(ServerConnectionOverlay).Assembly;
            }
            catch (InvalidOperationException)
            {
                // already pinned by the test host - individual tests verify the outcome
            }
        }

        #region Harness
        /// <summary>
        /// Runs the body on a dedicated STA thread with its own dispatcher (so the
        /// overlay's window, timer and animations work exactly like in the app) and
        /// rethrows any failure on the xunit thread.
        /// </summary>
        private static void RunOnStaThread(Action body)
        {
            Exception? error = null;

            Thread thread = new(() =>
            {
                try
                {
                    // the overlay thread in the real app is warmed up by the overlay's
                    // own constructor; the synthetic windows here need the same so
                    // post-Show Topmost changes reach the native band
                    ServerConnectionOverlay.WarmUpThread();

                    body();
                }
                catch (Exception ex)
                {
                    error = ex;
                }

                Dispatcher.CurrentDispatcher.InvokeShutdown();
            })
            {
                IsBackground = true,
                Name = "ServerConnectionOverlayHarness"
            };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            if (!thread.Join(TimeSpan.FromSeconds(90)))
                throw new TimeoutException("Overlay harness thread did not finish in time");

            if (error is not null)
                ExceptionDispatchInfo.Capture(error).Throw();
        }

        /// <summary>
        /// Pumps the dispatcher for the given duration so the overlay's 250 ms tracking
        /// timer, layout and animations get to run.
        /// </summary>
        private static void Pump(double milliseconds)
        {
            DispatcherFrame frame = new();

            DispatcherTimer timer = new(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(milliseconds)
            };

            timer.Tick += (_, _) =>
            {
                frame.Continue = false;
                timer.Stop();
            };

            timer.Start();
            Dispatcher.PushFrame(frame);
        }
        #endregion

        #region Native helpers
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public long Area => (long)Math.Max(0, Right - Left) * Math.Max(0, Bottom - Top);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int X;
            public int Y;
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOPMOST = 0x00000008;

        /// <summary>
        /// <c>GetWindow</c> command: the window immediately above the given window in the z-order.
        /// </summary>
        private const uint GW_HWNDPREV = 3;

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out NativeRect lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr hWnd, out NativeRect lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ClientToScreen(IntPtr hWnd, ref NativePoint lpPoint);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hWnd);

        private static double GetScale(IntPtr hWnd)
        {
            uint dpi = GetDpiForWindow(hWnd);
            return dpi > 0 ? dpi / 96.0 : 1.0;
        }

        /// <summary>
        /// Pumps the dispatcher until the condition holds or the timeout expires.
        /// Returns whether it became true.
        /// </summary>
        private static bool WaitUntil(Func<bool> condition, double timeoutMs)
        {
            DateTime deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

            while (DateTime.UtcNow < deadline)
            {
                if (condition())
                    return true;

                Pump(100);
            }

            return condition();
        }

        /// <summary>
        /// Where the overlay should sit, in physical screen coordinates, given its
        /// current layout size - mirrors the production positioning contract.
        /// </summary>
        private static (int X, int Y) ExpectedPosition(Window target, ServerConnectionOverlay overlay)
        {
            IntPtr targetHwnd = new WindowInteropHelper(target).Handle;

            Assert.True(GetClientRect(targetHwnd, out NativeRect clientRect), "GetClientRect failed");
            Assert.True(clientRect.Right > 0, "target client rect is empty");

            NativePoint origin = new();
            Assert.True(ClientToScreen(targetHwnd, ref origin), "ClientToScreen failed");

            double scale = GetScale(targetHwnd);
            double widthPx = overlay.ActualWidth * scale;

            int x = origin.X + (int)Math.Round((clientRect.Right - widthPx) / 2);
            int y = origin.Y + (int)Math.Round(TopOffsetDip * scale);

            return (x, y);
        }
        #endregion

        [Fact]
        public void OverlayIsPlacedAtTopCenterOfTargetWindow()
        {
            RunOnStaThread(() =>
            {
                // borderless, like the Roblox client
                Window target = new()
                {
                    Width = 800,
                    Height = 600,
                    Left = 150,
                    Top = 120,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false
                };

                ServerConnectionOverlay? overlay = null;

                try
                {
                    target.Show();
                    Pump(300);

                    IntPtr targetHwnd = new WindowInteropHelper(target).Handle;
                    Assert.NotEqual(IntPtr.Zero, targetHwnd);

                    overlay = new ServerConnectionOverlay { TargetWindowOverride = targetHwnd };
                    overlay.ShowNotification("Test City, Test State: Test Game", 0, 10);

                    Pump(800);

                    Assert.True(overlay.IsVisible, "overlay should be visible after ShowNotification");

                    Assert.True(GetWindowRect(new WindowInteropHelper(overlay).Handle, out NativeRect overlayRect), "GetWindowRect failed");
                    (int expectedX, int expectedY) = ExpectedPosition(target, overlay);

                    Assert.InRange(overlayRect.Left, expectedX - TolerancePx, expectedX + TolerancePx);
                    Assert.InRange(overlayRect.Top, expectedY - TolerancePx, expectedY + TolerancePx);
                }
                finally
                {
                    overlay?.Shutdown();
                    target.Close();
                    Pump(200);
                }
            });
        }

        /// <summary>
        /// The overlay has to end up in front of the target window. SetWindowPos inserts a
        /// window *after* hWndInsertAfter in the z-order, i.e. behind it - so passing the target
        /// handle, the obvious-looking choice, silently tucks the notification underneath the
        /// game window where it can never be seen.
        /// </summary>
        [Fact]
        public void OverlayIsPlacedAboveTheTargetWindow()
        {
            RunOnStaThread(() =>
            {
                Window target = new()
                {
                    Width = 800,
                    Height = 600,
                    Left = 150,
                    Top = 120,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false
                };

                ServerConnectionOverlay? overlay = null;

                try
                {
                    target.Show();
                    Pump(300);

                    IntPtr targetHwnd = new WindowInteropHelper(target).Handle;
                    Assert.NotEqual(IntPtr.Zero, targetHwnd);

                    overlay = new ServerConnectionOverlay { TargetWindowOverride = targetHwnd };
                    overlay.ShowNotification("Test City, Test State: Test Game", 0, 10);

                    Pump(400);

                    IntPtr overlayHwnd = new WindowInteropHelper(overlay).Handle;
                    Assert.NotEqual(IntPtr.Zero, overlayHwnd);
                    Assert.True(overlay.IsVisible, "overlay should be visible after ShowNotification");

                    // put the overlay back where the original bug left it: hidden behind the target
                    SetWindowPos(overlayHwnd, targetHwnd, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);

                    // no pumping here, so the overlay cannot have been repositioned yet
                    Assert.True(GetWindow(targetHwnd, GW_HWNDPREV) != overlayHwnd,
                        "test setup failed to push the overlay behind the target");

                    // the 250 ms tracking timer must put the overlay back in front of the target
                    Pump(700);

                    IntPtr windowAboveTarget = GetWindow(targetHwnd, GW_HWNDPREV);

                    Assert.True(windowAboveTarget == overlayHwnd,
                        $"the overlay must sit directly above the target window, but the window above it is 0x{windowAboveTarget.ToInt64():X} (overlay is 0x{overlayHwnd.ToInt64():X})");
                }
                finally
                {
                    overlay?.Shutdown();
                    target.Close();
                    Pump(200);
                }
            });
        }

        [Fact]
        public void OverlayFollowsTargetMoveAndResize()
        {
            RunOnStaThread(() =>
            {
                Window target = new()
                {
                    Width = 800,
                    Height = 600,
                    Left = 150,
                    Top = 120,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false
                };

                ServerConnectionOverlay? overlay = null;

                try
                {
                    target.Show();
                    Pump(300);

                    overlay = new ServerConnectionOverlay
                    {
                        TargetWindowOverride = new WindowInteropHelper(target).Handle
                    };

                    overlay.ShowNotification("Test City, Test State: Test Game", 0, 10);
                    Pump(600);

                    Assert.True(overlay.IsVisible, "overlay should be visible");

                    // move and resize the target well away from its original spot
                    target.Left = 400;
                    target.Top = 300;
                    target.Width = 1000;
                    Pump(800); // several 250 ms tracking ticks

                    Assert.True(overlay.IsVisible, "overlay should still be visible after target moved");
                    Assert.True(GetWindowRect(new WindowInteropHelper(overlay).Handle, out NativeRect overlayRect), "GetWindowRect failed");
                    (int expectedX, int expectedY) = ExpectedPosition(target, overlay);

                    Assert.InRange(overlayRect.Left, expectedX - TolerancePx, expectedX + TolerancePx);
                    Assert.InRange(overlayRect.Top, expectedY - TolerancePx, expectedY + TolerancePx);
                }
                finally
                {
                    overlay?.Shutdown();
                    target.Close();
                    Pump(200);
                }
            });
        }

        [Fact]
        public void OverlayHidesWhileTargetMinimizedAndReturnsOnRestore()
        {
            RunOnStaThread(() =>
            {
                Window target = new()
                {
                    Width = 800,
                    Height = 600,
                    Left = 150,
                    Top = 120,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false
                };

                ServerConnectionOverlay? overlay = null;

                try
                {
                    target.Show();
                    Pump(300);

                    overlay = new ServerConnectionOverlay
                    {
                        TargetWindowOverride = new WindowInteropHelper(target).Handle
                    };

                    overlay.ShowNotification("Test City, Test State: Test Game", 0, 10);
                    Pump(600);

                    Assert.True(overlay.IsVisible, "overlay should be visible before minimize");

                    target.WindowState = WindowState.Minimized;
                    Pump(800);

                    Assert.False(overlay.IsVisible, "overlay should hide while the target is minimized");

                    target.WindowState = WindowState.Normal;
                    Pump(800);

                    Assert.True(overlay.IsVisible, "overlay should reappear after the target is restored");
                    Assert.True(GetWindowRect(new WindowInteropHelper(overlay).Handle, out NativeRect overlayRect), "GetWindowRect failed");
                    (int expectedX, int expectedY) = ExpectedPosition(target, overlay);

                    Assert.InRange(overlayRect.Left, expectedX - TolerancePx, expectedX + TolerancePx);
                    Assert.InRange(overlayRect.Top, expectedY - TolerancePx, expectedY + TolerancePx);
                }
                finally
                {
                    overlay?.Shutdown();
                    target.Close();
                    Pump(200);
                }
            });
        }

        [Fact]
        public void OverlayAutoHidesAfterDurationAndStaysHidden()
        {
            RunOnStaThread(() =>
            {
                Window target = new()
                {
                    Width = 800,
                    Height = 600,
                    Left = 150,
                    Top = 120,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false
                };

                ServerConnectionOverlay? overlay = null;

                try
                {
                    target.Show();
                    Pump(300);

                    overlay = new ServerConnectionOverlay
                    {
                        TargetWindowOverride = new WindowInteropHelper(target).Handle
                    };

                    // 1 second is the minimum the overlay accepts (clamped to 1-30)
                    overlay.ShowNotification("Test City, Test State: Test Game", 0, 1);
                    Pump(600);

                    Assert.True(overlay.IsVisible, "overlay should be visible at first");

                    // 1 s expiry + 250 ms fade + tracking tick slack
                    Pump(2600);
                    Assert.False(overlay.IsVisible, "overlay should have faded out after its duration");

                    // and it must never resurrect itself afterwards
                    Pump(800);
                    Assert.False(overlay.IsVisible, "overlay must stay hidden after expiring");
                }
                finally
                {
                    overlay?.Shutdown();
                    target.Close();
                    Pump(200);
                }
            });
        }

        [Fact]
        public void OverlayUpdatesTextInPlaceWithoutRestartingItsTimer()
        {
            RunOnStaThread(() =>
            {
                Window target = new()
                {
                    Width = 800,
                    Height = 600,
                    Left = 150,
                    Top = 120,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false
                };

                ServerConnectionOverlay? overlay = null;

                try
                {
                    target.Show();
                    Pump(300);

                    overlay = new ServerConnectionOverlay
                    {
                        TargetWindowOverride = new WindowInteropHelper(target).Handle
                    };

                    // 2 s, so there is room to observe the original and the updated text
                    overlay.ShowNotification("Unknown location: Roblox", 0, 2);
                    Pump(500);

                    Assert.True(overlay.IsVisible, "overlay should be visible at first");
                    Assert.Equal("Unknown location: Roblox", overlay.NotificationText.Text);

                    // the resolved details get patched into the notification that is already up
                    overlay.UpdateNotification("Dallas, Texas: Brookhaven RP");
                    Pump(300);

                    Assert.True(overlay.IsVisible, "updating the text must not hide the overlay");
                    Assert.Equal("Dallas, Texas: Brookhaven RP", overlay.NotificationText.Text);

                    // and it must still expire on the original schedule
                    Pump(2600);
                    Assert.False(overlay.IsVisible, "updating the text must not extend the notification's lifetime");

                    // a lookup that lands after the notification faded must not resurrect it
                    overlay.UpdateNotification("Chicago, Illinois: Adopt Me!");
                    Pump(400);

                    Assert.False(overlay.IsVisible, "a late update must not resurrect an expired notification");
                    Assert.Equal("Dallas, Texas: Brookhaven RP", overlay.NotificationText.Text);
                }
                finally
                {
                    overlay?.Shutdown();
                    target.Close();
                    Pump(200);
                }
            });
        }

        [Fact]
        public void OverlayMirrorsTopmostStateOfTarget()
        {
            RunOnStaThread(() =>
            {
                Window target = new()
                {
                    Width = 800,
                    Height = 600,
                    Left = 150,
                    Top = 120,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false,
                    Topmost = false
                };

                ServerConnectionOverlay? overlay = null;

                try
                {
                    target.Show();
                    Pump(300);

                    IntPtr targetHwnd = new WindowInteropHelper(target).Handle;

                    overlay = new ServerConnectionOverlay { TargetWindowOverride = targetHwnd };
                    overlay.ShowNotification("Test City, Test State: Test Game", 0, 10);
                    Pump(600);

                    IntPtr overlayHwnd = new WindowInteropHelper(overlay).Handle;

                    Assert.True(overlay.IsVisible, "overlay should be visible");
                    Assert.Equal(0, GetWindowLong(overlayHwnd, GWL_EXSTYLE) & WS_EX_TOPMOST);

                    // a fullscreen game raises itself into the topmost band - the overlay
                    // has to follow so it can paint above the game
                    target.Topmost = true;

                    if (!WaitUntil(() => (GetWindowLong(targetHwnd, GWL_EXSTYLE) & WS_EX_TOPMOST) != 0, 2000))
                    {
                        // the synthetic target is itself a WPF window, whose own flip can
                        // occasionally be dropped - re-fire its change handling once
                        target.Topmost = false;
                        target.Topmost = true;
                    }

                    Assert.True(overlay.IsVisible, "overlay should stay visible when the target goes topmost");
                    Assert.True(target.Topmost, "target Topmost DP should be true");
                    Assert.True(
                        WaitUntil(() => (GetWindowLong(targetHwnd, GWL_EXSTYLE) & WS_EX_TOPMOST) != 0, 2000),
                        $"target native topmost bit should be set (exstyle=0x{GetWindowLong(targetHwnd, GWL_EXSTYLE):X})");
                    Assert.True(
                        WaitUntil(() => (GetWindowLong(overlayHwnd, GWL_EXSTYLE) & WS_EX_TOPMOST) != 0, 3000),
                        $"overlay should follow into the topmost band (exstyle=0x{GetWindowLong(overlayHwnd, GWL_EXSTYLE):X}, overlay.Topmost={overlay.Topmost})");

                    // and it must leave the band again once the target does
                    target.Topmost = false;
                    Assert.True(
                        WaitUntil(() => (GetWindowLong(overlayHwnd, GWL_EXSTYLE) & WS_EX_TOPMOST) == 0, 3000),
                        "overlay should leave the topmost band with the target");
                }
                finally
                {
                    overlay?.Shutdown();
                    target.Close();
                    Pump(200);
                }
            });
        }

        /// <summary>
        /// A game window that covers its monitor is presented through the fullscreen
        /// optimisation / independent flip path, which does not composite with windows that
        /// merely sit above it in the z-order - such a toast is invisible no matter how
        /// correctly it is positioned. The overlay has to join the topmost band for those, while
        /// a windowed target must still be left alone (covered by the test above).
        /// </summary>
        [Fact]
        public void OverlayJoinsTopmostBandWhenTargetCoversItsMonitor()
        {
            RunOnStaThread(() =>
            {
                // DIPs, so this stays the whole monitor whatever the display scaling is
                Window target = new()
                {
                    Width = SystemParameters.PrimaryScreenWidth,
                    Height = SystemParameters.PrimaryScreenHeight,
                    Left = 0,
                    Top = 0,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false,
                    Topmost = false
                };

                ServerConnectionOverlay? overlay = null;

                try
                {
                    target.Show();
                    Pump(400);

                    IntPtr targetHwnd = new WindowInteropHelper(target).Handle;

                    Assert.True(GetClientRect(targetHwnd, out NativeRect clientRect), "GetClientRect failed");

                    NativePoint origin = new() { X = 0, Y = 0 };
                    Assert.True(ClientToScreen(targetHwnd, ref origin), "ClientToScreen failed");

                    Assert.True(
                        origin.X <= 8 && origin.Y <= 8 &&
                        origin.X + clientRect.Right >= SystemParameters.PrimaryScreenWidth - 8 &&
                        origin.Y + clientRect.Bottom >= SystemParameters.PrimaryScreenHeight - 8,
                        $"test setup failed to cover the monitor (client {clientRect.Right}x{clientRect.Bottom} at {origin.X},{origin.Y})");

                    overlay = new ServerConnectionOverlay { TargetWindowOverride = targetHwnd };
                    overlay.ShowNotification("Test City, Test State: Test Game", 0, 10);
                    Pump(600);

                    IntPtr overlayHwnd = new WindowInteropHelper(overlay).Handle;

                    Assert.True(overlay.IsVisible, "overlay should be visible");
                    Assert.True(overlay.Topmost, "overlay should join the topmost band when the target covers its monitor");
                    Assert.True(
                        WaitUntil(() => (GetWindowLong(overlayHwnd, GWL_EXSTYLE) & WS_EX_TOPMOST) != 0, 3000),
                        $"overlay should be natively topmost (exstyle=0x{GetWindowLong(overlayHwnd, GWL_EXSTYLE):X})");
                }
                finally
                {
                    overlay?.Shutdown();
                    target.Close();
                    Pump(200);
                }
            });
        }

        [Fact]
        public void LargestVisibleWindowIsSelectedForTracking()
        {
            RunOnStaThread(() =>
            {
                Window small = new()
                {
                    Width = 300,
                    Height = 200,
                    Left = 50,
                    Top = 50,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false
                };

                Window large = new()
                {
                    Width = 700,
                    Height = 500,
                    Left = 400,
                    Top = 300,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false
                };

                try
                {
                    small.Show();
                    large.Show();
                    Pump(300);

                    IntPtr smallHwnd = new WindowInteropHelper(small).Handle;
                    IntPtr largeHwnd = new WindowInteropHelper(large).Handle;

                    IntPtr picked = ServerConnectionOverlay.FindLargestVisibleWindow(Process.GetCurrentProcess().Id);

                    Assert.NotEqual(IntPtr.Zero, picked);
                    Assert.NotEqual(smallHwnd, picked);

                    Assert.True(GetWindowRect(picked, out NativeRect pickedRect), "GetWindowRect failed");
                    Assert.True(GetWindowRect(largeHwnd, out NativeRect largeRect), "GetWindowRect failed");

                    Assert.True(
                        pickedRect.Area >= largeRect.Area,
                        $"expected a window at least as large as the {largeRect.Area}px one, got {pickedRect.Area}px");
                }
                finally
                {
                    small.Close();
                    large.Close();
                    Pump(200);
                }
            });
        }
    }
}
