using System.Runtime.InteropServices;

namespace Agentstrap.Utility
{
    /// <summary>
    /// Applies user-defined size, position and title to an already running window.
    /// Used to shape the Roblox player window after it has been created.
    /// </summary>
    internal static class WindowAttributes
    {
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);

        /// <summary>
        /// Resizes, moves and/or retitles a window. Any null argument is left
        /// untouched. Returns true if at least one attribute was applied.
        /// </summary>
        public static bool Apply(IntPtr hWnd, int? width, int? height, int? x, int? y, string? title)
        {
            if (hWnd == IntPtr.Zero)
                return false;

            bool hasSize = width is not null || height is not null;
            bool hasPosition = x is not null || y is not null;
            bool applied = false;

            if (hasSize || hasPosition)
            {
                uint flags = SWP_NOZORDER | SWP_NOACTIVATE;

                if (!hasSize)
                    flags |= SWP_NOSIZE;

                if (!hasPosition)
                    flags |= SWP_NOMOVE;

                applied = SetWindowPos(hWnd, IntPtr.Zero, x ?? 0, y ?? 0, width ?? 0, height ?? 0, flags) || applied;
            }

            if (!String.IsNullOrWhiteSpace(title))
                applied = SetWindowText(hWnd, title) || applied;

            return applied;
        }
    }
}
