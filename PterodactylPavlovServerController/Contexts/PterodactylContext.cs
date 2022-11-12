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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string? connectionString = this.configuration.GetConnectionString("Pterodactyl");
        if (connectionString == null)
        {
            throw new Exception("Connection string required");
        }
        optionsBuilder.UseMySQL(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PterodactylUserModel>().HasNoKey();
    }

    public DbSet<PterodactylUserModel> Users { get; set; }
}
