using Microsoft.EntityFrameworkCore;
using PterodactylPavlovServerController.Models;

namespace PterodactylPavlovServerController.Contexts;

public class PterodactylContext : DbContext
{
    private readonly IConfiguration configuration;

    public PterodactylContext(IConfiguration configuration)
    {
        this.configuration = configuration;
    }


    // AutoDetect opens a connection to the database to probe its version.
    // OnConfiguring runs for every context instance, so without this cache a
    // page that builds one context per row paid a round-trip per row before it
    // could render.
    private static readonly Dictionary<string, ServerVersion> serverVersionCache = new();

    private static ServerVersion getServerVersion(string connectionString)
    {
        lock (serverVersionCache)
        {
            if (!serverVersionCache.TryGetValue(connectionString, out ServerVersion? serverVersion))
            {
                serverVersion = ServerVersion.AutoDetect(connectionString);
                serverVersionCache[connectionString] = serverVersion;
            }

            return serverVersion;
        }
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string? connectionString = this.configuration.GetConnectionString("Pterodactyl");
        if (connectionString == null)
        {
            throw new Exception("Connection string required");
        }
        optionsBuilder.UseMySql(connectionString, getServerVersion(connectionString));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PterodactylUserModel>().HasNoKey();
    }

    public DbSet<PterodactylUserModel> Users { get; set; }
}
