using CommonLibrary.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using UserService.Entities;
using UserService.Migrations.Config;
using UserService.Services.UserManager;
using Npgsql;

namespace UserService.Services.Database
{
    public class DatabaseContext : IdentityDbContext<User>
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
            modelBuilder.Entity<TermsConditionsTranslation>().HasKey(table => new {
                table.LangKey,
                table.TermsConditionsVersion
            });
            modelBuilder.Entity<AssociationProfileTranslation>().HasKey(table => new {
                table.LangKey,
                table.PageContentKey
            });
            base.OnModelCreating(modelBuilder);
            new DbSeeder(modelBuilder).Seed();
        }

        public DbSet<User> User { get; set; }
        public DbSet<Association> Association { get; set; }
        public DbSet<AssociationType> AssociationType { get; set; }
        public DbSet<AssociationProfileTranslation> AssociationProfileTranslation { get; set; }
        public DbSet<TermsConditions> TermsConditions { get; set; }
        public DbSet<TermsConditionsTranslation> TermsConditionsTranslation { get; set; }
        public DbSet<Scope> Scope { get; set; }
        public DbSet<RoleScope> RoleScope { get; set; }
        public DbSet<Setting> Setting { get; set; }
        public DbSet<ClientCredential> ClientCredential { get; set; }
        public DbSet<NewsletterSubscription> NewsletterSubscription { get; set; }
    }
}
