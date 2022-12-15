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
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
