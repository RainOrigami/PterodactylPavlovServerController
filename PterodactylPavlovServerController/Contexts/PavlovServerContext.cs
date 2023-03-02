using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PavlovVR_Rcon.Models.Pavlov;
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
    public DbSet<AuditActionModel> AuditActions { get; set; }
    public DbSet<MapRotationModel> MapRotations { get; set; }
    public DbSet<ServerMapModel> Maps { get; set; }
    public DbSet<ServerSettings> Settings { get; set; }
    public DbSet<ServerWarmupItemModel> WarmupItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = this.configuration.GetConnectionString("PavlovServers")!;
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersistentPavlovPlayerModel>().HasKey(p => new
        {
            p.UniqueId,
            p.ServerId,
        });

        modelBuilder.Entity<AuditActionModel>().HasKey(a => a.Id);

        modelBuilder.Entity<ServerMapModel>().HasKey(m => new { m.MapLabel, m.GameMode });
        modelBuilder.Entity<MapRotationModel>().HasKey(r => new { r.ServerId, r.Name });
        modelBuilder.Entity<ServerSettings>().HasKey(s => new { s.ServerId, s.SettingName });
        modelBuilder.Entity<ServerWarmupItemModel>().HasKey(w => new { w.ServerId, w.Item });
        modelBuilder.Entity<ServerWarmupItemModel>().Property(w => w.Item).HasConversion(new EnumToStringConverter<Item>());

        modelBuilder.Entity<MapRotationModel>()
            .HasMany(r => r.Maps)
            .WithMany(m => m.Rotations)
            .UsingEntity<MapInMapRotationModel>(
                j => j.HasOne(pm => pm.Map)
                    .WithMany(m => m.MapsInRotation)
                    .HasForeignKey(pm => new { pm.MapLabel, pm.GameMode }),
                j => j.HasOne(pr => pr.Rotation)
                .WithMany(r => r.MapsInRotation)
                .HasForeignKey(pr => new { pr.ServerId, pr.RotationName }),
                j => j.ToTable("MapsInRotation").HasKey(t => new { t.MapLabel, t.GameMode, t.ServerId, t.RotationName, t.Order }));
    }
}
