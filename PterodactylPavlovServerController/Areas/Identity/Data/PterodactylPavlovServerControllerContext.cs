using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PterodactylPavlovServerController.Areas.Identity.Data;

namespace PterodactylPavlovServerController.Data;

public class PterodactylPavlovServerControllerContext : IdentityDbContext<PterodactylPavlovServerControllerUser>
{
    private readonly IConfiguration configuration;

    public PterodactylPavlovServerControllerContext(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = this.configuration.GetConnectionString("PPSC")!;
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

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
