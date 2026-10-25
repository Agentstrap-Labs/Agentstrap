using System.Collections.Concurrent;

namespace Agentstrap.Integrations
{
    /// <summary>
    /// Suppresses repeated notifications for the same connection/endpoint within a
    /// configurable window. In-memory only: restarting Agentstrap starts fresh, and
    /// no connection history is ever persisted.
    /// </summary>
    public sealed class NotificationRateLimiter
    {
        private readonly TimeSpan _window;
        private readonly Func<DateTimeOffset> _clock;
        private readonly ConcurrentDictionary<string, DateTimeOffset> _lastSent = new(StringComparer.OrdinalIgnoreCase);

        public NotificationRateLimiter(TimeSpan? window = null, Func<DateTimeOffset>? clock = null)
        {
            _window = window ?? TimeSpan.FromMinutes(30);
            _clock = clock ?? (() => DateTimeOffset.UtcNow);
        }

        public static NotificationRateLimiter FromMinutes(int minutes, Func<DateTimeOffset>? clock = null) =>
            new(TimeSpan.FromMinutes(Math.Max(0, minutes)), clock);

        public TimeSpan Window => _window;

        /// <summary>
        /// Returns whether a notification for <paramref name="key"/> may be sent right now.
        /// </summary>
        public bool ShouldSend(string key) => !IsSuppressed(key, out _);

        public bool IsSuppressed(string key, out TimeSpan remaining)
        {
            remaining = TimeSpan.Zero;

            if (_window <= TimeSpan.Zero || String.IsNullOrWhiteSpace(key))
                return false;

            if (!_lastSent.TryGetValue(key, out DateTimeOffset sentAt))
                return false;

            remaining = sentAt + _window - _clock();

            if (remaining > TimeSpan.Zero)
                return true;

            _lastSent.TryRemove(key, out _);
            return false;
        }

        public void MarkSent(string key)
        {
            if (String.IsNullOrWhiteSpace(key))
                return;

            _lastSent[key] = _clock();
            Prune();
        }

        /// <summary>Forgets every recorded notification; mostly useful for tests.</summary>
        public void Reset() => _lastSent.Clear();

        public int TrackedCount => _lastSent.Count;

        public DateTime? GetLastSent(string key) =>
            _lastSent.TryGetValue(key, out DateTimeOffset sentAt) ? sentAt.UtcDateTime : null;

        /// <summary>
        /// Drops stale entries as we go, so the dictionary cannot grow without bound.
        /// </summary>
        private void Prune()
        {
            if (_lastSent.Count < 64)
                return;

            DateTimeOffset now = _clock();

            foreach (var pair in _lastSent)
            {
                if (pair.Value + _window <= now)
                    _lastSent.TryRemove(pair.Key, out _);
            }
        }
    }
}
