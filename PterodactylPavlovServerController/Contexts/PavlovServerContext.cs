using Microsoft.EntityFrameworkCore;
using PterodactylPavlovServerDomain.Models;

namespace PterodactylPavlovServerController.Contexts;

public class PavlovServerContext : DbContext
{
    private readonly IConfiguration configuration;

#pragma warning disable CS8618
    public PavlovServerContext(IConfiguration configuration)
#pragma warning restore CS8618
    {
        this.configuration = configuration;
    }

    public DbSet<PersistentPavlovPlayerModel> Players { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = this.configuration.GetConnectionString("PavlovServers")!;
        switch (this.configuration["db_type"])
        {
            case "sqlserver":
                optionsBuilder.UseSqlServer(connectionString);
                break;
            case "mysql":
                optionsBuilder.UseMySQL(connectionString);
                break;
            case "sqlite":
                optionsBuilder.UseSqlite(connectionString);
                break;
            default:
                throw new Exception("Invalid database type provided. Valid DB types are: sqlserver, mysql, sqlite");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersistentPavlovPlayerModel>().HasKey(p => new
        {
            p.UniqueId,
            p.ServerId,
        });
    }
}
