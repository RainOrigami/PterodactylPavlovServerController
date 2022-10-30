using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PavlovStatsReader.Models;

namespace PavlovStatsReader
{
    public class StatsContext : DbContext
    {
        private readonly IConfiguration configuration;

        public DbSet<KillData> KillData { get; set; }
        public DbSet<EndOfMapStats> EndOfMapStats { get; set; }
        public DbSet<PlayerStats> PlayerStats { get; set; }
        public DbSet<BombData> BombData { get; set; }
        public DbSet<SwitchTeam> SwitchTeams { get; set; }
        public DbSet<RoundState> RoundStates { get; set; }
        public DbSet<RoundEnd> RoundEnds { get; set; }

        public DbSet<Setting> Settings { get; set; }

        public StatsContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = configuration.GetConnectionString("PavlovStats");
            switch (configuration["db_type"])
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

            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EndOfMapStats>().HasMany(eoms => eoms.PlayerStats).WithOne(ps => ps.EndOfMapStats);
            modelBuilder.Entity<PlayerStats>().HasMany(ps => ps.Stats).WithOne(s => s.PlayerStats);
            modelBuilder.Entity<Setting>().HasKey(s => new { s.ServerId, s.Name });
        }
    }
}
