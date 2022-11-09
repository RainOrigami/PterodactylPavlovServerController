using System.Collections.Concurrent;
using PterodactylPavlovServerController.Models;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Services;

public class PavlovRconConnectionService : IDisposable
{
    public delegate void ServersUpdated();

    private readonly IConfiguration configuration;

    private readonly ConcurrentDictionary<string, PavlovRconConnection> connections = new();
    private readonly PavlovRconService pavlovRconService;
    private readonly PterodactylService pterodactylService;

    private readonly CancellationTokenSource updaterCancellationTokenSource = new();

    public PavlovRconConnectionService(PterodactylService pterodactylService, PavlovRconService pavlovRconService, IConfiguration configuration)
    {
        this.pterodactylService = pterodactylService;
        this.pavlovRconService = pavlovRconService;
        this.configuration = configuration;
    }

    public bool Initialised { get; private set; }

    public void Dispose()
    {
        this.updaterCancellationTokenSource.Cancel();
    }

    public PavlovRconConnection[] GetAllConnections()
    {
        return this.connections.Values.ToArray();
    }

    public PavlovRconConnection? GetServer(string serverId)
    {
        this.connections.TryGetValue(serverId, out PavlovRconConnection? connection);
        return connection;
    }

    public event ServersUpdated? OnServersUpdated;

    public void Run()
    {
        Task.Run(this.serverUpdater);
    }

    private async Task serverUpdater()
    {
        while (!this.updaterCancellationTokenSource.Token.IsCancellationRequested)
        {
            PterodactylServerModel[] serverModels = this.pterodactylService.GetServers();
            serverModels.Where(s => !this.connections.ContainsKey(s.ServerId)).AsParallel().ForAll(this.addServer);
            this.Initialised = true;
            this.OnServersUpdated?.Invoke();

            await Task.Delay(1000);
        }
    }

    private void addServer(PterodactylServerModel server)
    {
        PavlovRconConnection serverConnection = new(server, this.pavlovRconService, this.configuration);
        serverConnection.Run();
        this.connections.AddOrUpdate(server.ServerId, serverConnection, (k, v) => serverConnection);
    }
}
