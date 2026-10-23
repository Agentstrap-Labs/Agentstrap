using System.Web;
using System.Windows;
using System.Windows.Input;
using Agentstrap.AppData;
using Agentstrap.Models.APIs;
using CommunityToolkit.Mvvm.Input;

namespace Agentstrap.Models.Entities
{
    public class ActivityData
    {
        private long _universeId = 0;

        /// <summary>
        /// If the current activity stems from an in-universe teleport, then this will be
        /// set to the activity that corresponds to the initial game join
        /// </summary>
        public ActivityData? RootActivity;

        public long UniverseId
        {
            get => _universeId;
            set
            {
                _universeId = value;
                UniverseDetails.LoadFromCache(value);
            }
        }

        public long PlaceId { get; set; } = 0;

        public string JobId { get; set; } = string.Empty;

        /// <summary>
        /// This will be empty unless the server joined is a private server
        /// </summary>
        public string AccessCode { get; set; } = string.Empty;
        
        public long UserId { get; set; } = 0;

        public string MachineAddress { get; set; } = string.Empty;

        public bool MachineAddressValid => !string.IsNullOrEmpty(MachineAddress) && !MachineAddress.StartsWith("10.");

        public bool IsTeleport { get; set; } = false;

        public ServerType ServerType { get; set; } = ServerType.Public;

        public DateTime TimeJoined { get; set; }

        public DateTime? TimeLeft { get; set; }

        // everything below here is optional strictly for bloxstraprpc, discord rich presence, or game history

        /// <summary>
        /// This is intended only for other people to use, i.e. context menu invite link, rich presence joining
        /// </summary>
        public string RPCLaunchData { get; set; } = string.Empty;

        public UniverseDetails? UniverseDetails { get; set; }

        public string GameHistoryDescription
        {
            get
            {
                string desc = string.Format(
                    "{0} • {1} {2} {3}", 
                    UniverseDetails?.Data.Creator.Name,
                    TimeJoined.ToString("t"), 
                    Locale.CurrentCulture.Name.StartsWith("ja") ? '~' : '-',
                    TimeLeft?.ToString("t")
                );

                if (ServerType != ServerType.Public)
                    desc += " • " + ServerType.ToTranslatedString();

                return desc;
            }
        }

        public ICommand RejoinServerCommand => new RelayCommand(RejoinServer);

        private SemaphoreSlim serverQuerySemaphore = new(1, 1);

        public string GetInviteDeeplink(bool launchData = true)
        {
            string deeplink = $"roblox://experiences/start?placeId={PlaceId}";

            if (ServerType == ServerType.Private)
                deeplink += "&accessCode=" + AccessCode;
            else
                deeplink += "&gameInstanceId=" + JobId;

            if (launchData && !string.IsNullOrEmpty(RPCLaunchData))
                deeplink += "&launchData=" + HttpUtility.UrlEncode(RPCLaunchData);

            return deeplink;
        }

        public async Task<string?> QueryServerLocation()
        {
            if (!MachineAddressValid)
                throw new InvalidOperationException($"Machine address is invalid ({MachineAddress})");

            IPInfoResponse? ipInfo = await QueryIpInfo();

            if (ipInfo is null)
                return null;

            if (ipInfo.City == ipInfo.Region)
                return $"{ipInfo.Region}, {ipInfo.Country}";
            else
                return $"{ipInfo.City}, {ipInfo.Region}, {ipInfo.Country}";
        }

        /// <summary>
        /// Returns the server location in "City, State" form, or null if it cannot be
        /// determined reliably enough (e.g. the lookup failed, or no distinct city was reported).
        /// </summary>
        public async Task<string?> QueryServerCityState()
        {
            IPInfoResponse? ipInfo = await QueryIpInfo();

            if (ipInfo is null)
                return null;

            if (string.IsNullOrEmpty(ipInfo.City) || string.IsNullOrEmpty(ipInfo.Region))
                return null;

            // ipinfo.io reports the region name as the city when it has nothing more precise
            // to offer - without a distinct city, we can't honestly claim a city and state
            if (ipInfo.City == ipInfo.Region)
                return null;

            return $"{ipInfo.City}, {ipInfo.Region}";
        }

        /// <summary>
        /// Fetches (or returns the already cached) ipinfo.io record for the server address.
        /// Returns null if the address isn't publicly queryable or the lookup failed; failed
        /// lookups are cached too, so we don't keep requerying for servers we can't locate.
        /// </summary>
        private async Task<IPInfoResponse?> QueryIpInfo()
        {
            const string LOG_IDENT = "ActivityData::QueryIpInfo";

            if (!MachineAddressValid)
                return null;

            await serverQuerySemaphore.WaitAsync();

            try
            {
                if (GlobalCache.ServerIpInfo.TryGetValue(MachineAddress, out IPInfoResponse? cachedIpInfo))
                    return cachedIpInfo;

                IPInfoResponse? ipInfo = null;

                try
                {
                    var response = await Http.GetJson<IPInfoResponse>($"https://ipinfo.io/{MachineAddress}/json");

                    if (string.IsNullOrEmpty(response.City))
                        throw new InvalidHTTPResponseException("Reported city was blank");

                    ipInfo = response;
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Failed to get server location for {MachineAddress}");
                    App.Logger.WriteException(LOG_IDENT, ex);
                }

                GlobalCache.ServerIpInfo[MachineAddress] = ipInfo;

                return ipInfo;
            }
            finally
            {
                serverQuerySemaphore.Release();
            }
        }

        public override string ToString() => $"{PlaceId}/{JobId}";

        private void RejoinServer()
        {
            string playerPath = new RobloxPlayerData().ExecutablePath;

            Process.Start(playerPath, GetInviteDeeplink(false));
        }
    }
}

