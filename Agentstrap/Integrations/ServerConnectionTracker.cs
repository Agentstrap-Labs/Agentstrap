namespace Agentstrap.Integrations
{
    /// <summary>
    /// Tracks Roblox server connection events for the server connection notification overlay.
    /// Filters out repeated connection events relating to the server that is already active,
    /// and lets asynchronous lookups that were started for an older connection be discarded.
    /// </summary>
    public class ServerConnectionTracker
    {
        private readonly object _lock = new();

        private string? _lastJobId = null;
        private string? _lastAddress = null;

        /// <summary>
        /// Set when the active server was left, meaning the next connection event is a
        /// genuine new connection even if it points at the same server as before.
        /// </summary>
        private bool _leftActiveServer = true;

        private int _generation = 0;

        /// <summary>
        /// Identifies the current connection. Bumped whenever a new connection begins
        /// or the current one ends.
        /// </summary>
        public int Generation
        {
            get
            {
                lock (_lock)
                    return _generation;
            }
        }

        /// <summary>
        /// Registers a newly joined server. Returns false if this is a repeated event
        /// for the server that is already active. On success, <paramref name="generation"/>
        /// identifies this connection for use with <see cref="IsCurrent(int)"/>.
        /// </summary>
        public bool TryTrackConnection(string jobId, string address, out int generation)
        {
            lock (_lock)
            {
                if (!_leftActiveServer && jobId == _lastJobId && address == _lastAddress)
                {
                    generation = _generation;
                    return false;
                }

                _lastJobId = jobId;
                _lastAddress = address;
                _leftActiveServer = false;

                generation = ++_generation;
                return true;
            }
        }

        /// <summary>
        /// Registers that the active server was left (or the connection was interrupted),
        /// invalidating any lookups still in flight for it.
        /// </summary>
        public void TrackLeave()
        {
            lock (_lock)
            {
                _leftActiveServer = true;
                _generation++;
            }
        }

        /// <summary>
        /// Returns whether the given generation still corresponds to the current connection.
        /// </summary>
        public bool IsCurrent(int generation)
        {
            lock (_lock)
                return generation == _generation;
        }
    }
}
