using PterodactylPavlovServerController.Models;
using PterodactylPavlovServerDomain.Models;
using System.Collections.Concurrent;

namespace PterodactylPavlovServerController.Services
{
    public class PavlovRconConnectionService : IDisposable
    {
        private readonly PterodactylService pterodactylService;
        private readonly PavlovServerService pavlovServerService;
        private readonly PavlovRconService pavlovRconService;
        private readonly SteamService steamService;
        private readonly SteamWorkshopService steamWorkshopService;
        private readonly IConfiguration configuration;

        private readonly ConcurrentDictionary<string, PavlovRconConnection> connections = new ConcurrentDictionary<string, PavlovRconConnection>();

        private readonly CancellationTokenSource updaterCancellationTokenSource = new();

        public bool Initialised { get; private set; } = false;

        public PavlovRconConnection[] GetAllConnections() => connections.Values.ToArray();

        public PavlovRconConnection? GetServer(string serverId)
        {
            connections.TryGetValue(serverId, out PavlovRconConnection? connection);
            return connection;
        }

        public delegate void ServersUpdated();

        public event ServersUpdated? OnServersUpdated;

        public PavlovRconConnectionService(PterodactylService pterodactylService, PavlovServerService pavlovServerService, PavlovRconService pavlovRconService, SteamService steamService, SteamWorkshopService steamWorkshopService, IConfiguration configuration)
        {
            this.pterodactylService = pterodactylService;
            this.pavlovServerService = pavlovServerService;
            this.pavlovRconService = pavlovRconService;
            this.steamService = steamService;
            this.steamWorkshopService = steamWorkshopService;
            this.configuration = configuration;
        }

        public void Run()
        {
            Task.Run(serverUpdater);
        }

        private async Task serverUpdater()
        {
            while (!updaterCancellationTokenSource.Token.IsCancellationRequested)
            {
                PterodactylServerModel[] serverModels = pterodactylService.GetServers();
                serverModels.Where(s => !connections.ContainsKey(s.ServerId)).AsParallel().ForAll(addServer);
                Initialised = true;
                OnServersUpdated?.Invoke();

                await Task.Delay(60 * 1000);
            }
        }

        private void addServer(PterodactylServerModel server)
        {
            PavlovRconConnection serverConnection = new PavlovRconConnection(server, pavlovRconService, configuration);
            serverConnection.Run();
            connections.AddOrUpdate(server.ServerId, serverConnection, (k, v) => serverConnection);
            serverConnection.OnPlayerDetailUpdated += (id, detail) => OnServersUpdated?.Invoke();
            serverConnection.OnPlayerListUpdated += id => OnServersUpdated?.Invoke();
            serverConnection.OnServerInfoUpdated += id => OnServersUpdated?.Invoke();
            serverConnection.OnServerOnlineStateChanged += id => OnServersUpdated?.Invoke();
        }

        public void Dispose()
        {
            updaterCancellationTokenSource.Cancel();
        }
    }
}
