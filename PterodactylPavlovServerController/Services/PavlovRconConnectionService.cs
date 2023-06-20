using PterodactylPavlovServerDomain.Models;
using System.Collections.Concurrent;

namespace PterodactylPavlovServerController.Services;

public class PavlovRconConnectionService : IDisposable
{
    public delegate void ServersUpdated();

    private readonly IConfiguration configuration;

    private readonly ConcurrentDictionary<string, PavlovRconConnection> connections = new();
    private readonly PavlovRconService pavlovRconService;
    private readonly PavlovServerService pavlovServerService;
    private readonly SteamService steamService;
    private readonly PterodactylService pterodactylService;
    private readonly AuditService auditService;
    private readonly Dictionary<string, List<string>> apiKeysToServerIds = new();
    private readonly ConcurrentDictionary<string, ReservedSlotService> reservedSlots = new();
    private readonly ConcurrentDictionary<string, WarmupRoundService> warmupRounds = new();
    private readonly ConcurrentDictionary<string, PauseServerService> pauses = new();
    private readonly ConcurrentDictionary<string, SkinByKillCountSetterService> skins = new();
    private readonly ConcurrentDictionary<string, PavlovPingLimiterService> pingLimiter = new();
    private readonly ConcurrentDictionary<string, RotateMapsRotationService> mapsRotation = new();
    private readonly CancellationTokenSource updaterCancellationTokenSource = new();

    public PavlovRconConnectionService(PterodactylService pterodactylService, PavlovRconService pavlovRconService, SteamService steamService, IServiceProvider serviceProvider, IConfiguration configuration, PavlovServerService pavlovServerService)
    {
        this.pterodactylService = pterodactylService;
        this.pavlovRconService = pavlovRconService;
        this.steamService = steamService;
        this.configuration = configuration;
        this.auditService = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<AuditService>();
        this.pavlovServerService = pavlovServerService;
    }

    public bool Initialised { get; private set; }

    public void Dispose()
    {
        this.updaterCancellationTokenSource.Cancel();
    }

    public PavlovRconConnection[] GetAllConnections(string apiKey)
    {
        string[] authorisedServers;
        lock (this.apiKeysToServerIds)
        {
            if (!this.apiKeysToServerIds.ContainsKey(apiKey))
            {
                this.apiKeysToServerIds.Add(apiKey, this.pterodactylService.GetServers(apiKey).Select(s => s.ServerId).ToList());
            }

            authorisedServers = this.apiKeysToServerIds[apiKey].ToArray();
        }

        return this.connections.Values.Where(c => authorisedServers.Contains(c.ServerId)).ToArray();
    }

    public PavlovRconConnection? GetServer(string apiKey, string serverId)
    {
        bool authorised = false;
        lock (this.apiKeysToServerIds)
        {
            if (!this.apiKeysToServerIds.ContainsKey(apiKey))
            {
                this.apiKeysToServerIds.Add(apiKey, this.pterodactylService.GetServers(apiKey).Select(s => s.ServerId).ToList());
            }

            authorised = this.apiKeysToServerIds[apiKey].Contains(serverId);
        }

        if (!authorised)
        {
            return null;
        }

        this.connections.TryGetValue(serverId, out PavlovRconConnection? connection);
        return connection;
    }

    public event ServersUpdated? OnServersUpdated;

    public void Run()
    {
        Task.Run(this.serverUpdater);
    }

    private DateTime lastFullRefresh = DateTime.MinValue;
    private PterodactylServerModel[]? pterodactylServers;

    private async Task serverUpdater()
    {
        while (!this.updaterCancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                if (this.lastFullRefresh < DateTime.Now.AddMinutes(-1))
                {
                    lock (this.apiKeysToServerIds)
                    {
                        this.apiKeysToServerIds.Clear();
                    }

                    this.pterodactylServers = null;
                    this.lastFullRefresh = DateTime.Now;
                }

                this.pterodactylServers ??= this.pterodactylService.GetServers(this.configuration["pterodactyl_apikey"]!);

                this.pterodactylServers.Where(s => !this.connections.ContainsKey(s.ServerId)).AsParallel().ForAll(this.addServer);
                this.Initialised = true;
                this.OnServersUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            await Task.Delay(1000);
        }
    }

    private void addServer(PterodactylServerModel server)
    {
        PavlovRconConnection serverConnection = new(this.configuration["pterodactyl_apikey"]!, server, this.pavlovRconService, this.steamService, this.configuration);
        serverConnection.Run();
        this.connections.AddOrUpdate(server.ServerId, serverConnection, (k, v) => serverConnection);
        ReservedSlotService reservedSlotService = new(this.configuration["pterodactyl_apikey"]!, serverConnection, this.pavlovRconService, this.configuration);
        this.reservedSlots.AddOrUpdate(server.ServerId, reservedSlotService, (k, v) => reservedSlotService);
        WarmupRoundService warmupRoundService = new(this.configuration["pterodactyl_apikey"]!, serverConnection, this.pavlovRconService, this.configuration);
        this.warmupRounds.AddOrUpdate(server.ServerId, warmupRoundService, (k, v) => warmupRoundService);
        PauseServerService pauseServerService = new(this.configuration["pterodactyl_apikey"]!, serverConnection, this.pavlovRconService, this.configuration);
        this.pauses.AddOrUpdate(server.ServerId, pauseServerService, (k, v) => pauseServerService);
        SkinByKillCountSetterService skinByKillCountSetterService = new(this.configuration["pterodactyl_apikey"]!, serverConnection, this.pavlovRconService, this.configuration);
        this.skins.AddOrUpdate(server.ServerId, skinByKillCountSetterService, (k, v) => skinByKillCountSetterService);
        PavlovPingLimiterService pavlovPingLimiterService = new(this.configuration["pterodactyl_apikey"]!, serverConnection, this.pavlovRconService, this.auditService, this.configuration);
        this.pingLimiter.AddOrUpdate(server.ServerId, pavlovPingLimiterService, (k, v) => pavlovPingLimiterService);
        RotateMapsRotationService rotateMapsRotationService = new(this.configuration["pterodactyl_apikey"]!, serverConnection, this.pavlovServerService);
        this.mapsRotation.AddOrUpdate(server.ServerId, rotateMapsRotationService, (k, v) => rotateMapsRotationService);
    }
    public ReservedSlotService? GetReservedSlotService(string serverId) => this.reservedSlots[serverId];
}
