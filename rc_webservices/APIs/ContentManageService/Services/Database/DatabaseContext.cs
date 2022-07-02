using ContentManageService.Entities;
using ContentManageService.Migrations.Config;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ContentManageService.Services.Database
{
    public class DatabaseContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public DatabaseContext(DbContextOptions<DatabaseContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // read: https://bartwullems.blogspot.com/2021/03/kubernetesoverride-appsettingsjson-file.html
            // read: https://sudonull.com/post/108-Working-with-the-Managed-Databases-service-from-Digital-Ocean-in-NET-Core
            // define the database to use
            var connStringBuilder = new NpgsqlConnectionStringBuilder();
            int dbPort = 0;
            int.TryParse(_configuration["DatabaseSettings:DbPort"], out dbPort);
            bool dbPooling = false;
            bool.TryParse(_configuration["DatabaseSettings:DbPooling"], out dbPooling);
            bool dbTrustCrt = false;
            bool.TryParse(_configuration["DatabaseSettings:DbTrustCertificate"], out dbTrustCrt);
            connStringBuilder.Host = _configuration["DatabaseSettings:DbHost"];
            connStringBuilder.Port = dbPort;
            connStringBuilder.SslMode = _configuration["DatabaseSettings:DbSslMode"] == "None" ? SslMode.Disable : SslMode.Require;
            connStringBuilder.Username = _configuration["DatabaseSettings:DbUser"];
            connStringBuilder.Password = _configuration["DatabaseSettings:DbPassword"];
            connStringBuilder.Database = _configuration["DatabaseSettings:Database"];
            connStringBuilder.TrustServerCertificate = dbTrustCrt;
            connStringBuilder.Pooling = dbPooling;
            connStringBuilder.ServerCompatibilityMode = _configuration["DatabaseSettings:DbServerCompatibilityMode"] == "Redshift" ? ServerCompatibilityMode.Redshift : ServerCompatibilityMode.None;
            optionsBuilder.UseNpgsql(connStringBuilder.ConnectionString, options => options.EnableRetryOnFailure(3));
            //optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnectionString"), options => options.EnableRetryOnFailure(3));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<BannerTranslation>().HasKey(table => new {
            //    table.LangKey,
            //    table.PageKey
            //});
            base.OnModelCreating(modelBuilder);
            new DbSeeder(modelBuilder).Seed();
        }

        public DbSet<Banner> Banner { get; set; }
        public DbSet<BannerTranslation> BannerTranslation { get; set; }
    }
}
