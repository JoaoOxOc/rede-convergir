using CommonLibrary.Entities;
using CommonLibrary.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Entities;
using UserService.Enum;

namespace UserService.Migrations.Config
{
    public class DbSeeder
    {
        private readonly ModelBuilder _modelBuilder;

        public DbSeeder(ModelBuilder modelBuilder)
        {
            this._modelBuilder = modelBuilder;
        }
        
        
        /// <summary>
        /// Seeds database with data. It creates an insert script when the migration is generated
        /// </summary>
        public void Seed()
        {
            #region Roles

            _modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "244d3f1a-8594-4adb-9c59-5ec36fcdbf03", Name = "AssociationAdmin", NormalizedName = "ASSOCIATIONADMIN" },
                new IdentityRole { Id = "179642d9-0f10-4d7d-a1a0-b485b3f6659f", Name = "User", NormalizedName = "USER" },
                new IdentityRole { Id = "69d5274f-235d-4013-bbac-0c4eddf31ecc", Name = "AssociationUser", NormalizedName = "ASSOCIATIONUSER" }
            );

            #endregion

            #region Scopes

            _modelBuilder.Entity<Scope>().HasData(
                new Scope { Id = 1, ScopeName = "user.read", Description = "Read only of he's own user data" },
                new Scope { Id = 2, ScopeName = "user.write", Description = "Read/write of he's own user data" },
                new Scope { Id = 3, ScopeName = "users.write", Description = "Read/write of users data" },
                new Scope { Id = 4, ScopeName = "associationusers.write", Description = "Read/write of association users data" },
                new Scope { Id = 5, ScopeName = "client.admin", Description = "Read/write of client apps data" },
                new Scope { Id = 6, ScopeName = "settings.admin", Description = "Read/write of API settings" },
                new Scope { Id = 7, ScopeName = "email.admin", Description = "Read/write of Email service data like templates" },
                new Scope { Id = 8, ScopeName = "cms.read", Description = "Read only of content management system data" },
                new Scope { Id = 9, ScopeName = "cms.write", Description = "Read/write of content management system data" },
                new Scope { Id = 10, ScopeName = "terms.admin", Description = "Read/write of terms & conditions records" },
                new Scope { Id = 11, ScopeName = "roles.admin", Description = "Read/write of users roles" },
                new Scope { Id = 12, ScopeName = "scopes.admin", Description = "Read/write of roles scopes" },
                new Scope { Id = 13, ScopeName = "newsletter.admin", Description = "Read/write of newsletter subscriptions" },
                new Scope { Id = 14, ScopeName = "discourse.admin", Description = "Discourse admin identity" },
                new Scope { Id = 15, ScopeName = "discourse.user", Description = "Discourse user identity" },
                new Scope { Id = 16, ScopeName = "association.admin", Description = "Read/write of association profile data" },
                new Scope { Id = 17, ScopeName = "newsblog.admin", Description = "Administration of news blog service API" },
                new Scope { Id = 18, ScopeName = "newsblog.post", Description = "User can post on news blog service API" },
                new Scope { Id = 19, ScopeName = "associationusers.read", Description = "Read of association users data" },
                new Scope { Id = 20, ScopeName = "association.read", Description = "Read of association profile data" }
            );

            #endregion

            #region RoleScopes

            _modelBuilder.Entity<RoleScope>().HasData(
                new RoleScope
                {
                    Id = 1,
                    RoleId = "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                    ScopeId = 1
                },
                new RoleScope
                {
                    Id = 2,
                    RoleId = "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                    ScopeId = 1
                },
                new RoleScope
                {
                    Id = 3,
                    RoleId = "179642d9-0f10-4d7d-a1a0-b485b3f6659f",
                    ScopeId = 1
                },
                new RoleScope
                {
                    Id = 4,
                    RoleId = "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                    ScopeId = 1
                },
                new RoleScope
                {
                    Id = 5,
                    RoleId = "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                    ScopeId = 2
                },
                new RoleScope
                {
                    Id = 6,
                    RoleId = "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                    ScopeId = 2
                },
                new RoleScope
                {
                    Id = 7,
                    RoleId = "179642d9-0f10-4d7d-a1a0-b485b3f6659f",
                    ScopeId = 2
                },
                new RoleScope
                {
                    Id = 8,
                    RoleId = "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                    ScopeId = 2
                },
                new RoleScope
                {
                    Id = 9,
                    RoleId = "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                    ScopeId = 3
                },
                new RoleScope
                {
                    Id = 10,
                    RoleId = "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                    ScopeId = 4
                },
                new RoleScope
                {
                    Id = 11,
                    RoleId = "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                    ScopeId = 4
                },
                new RoleScope
                {
                    Id = 12,
                    RoleId = "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                    ScopeId = 5
                },
                new RoleScope
                {
                    Id = 13,
                    RoleId = "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                    ScopeId = 6
                },
                new RoleScope
                {
                    Id = 14,
                    RoleId = "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                    ScopeId = 7
                },
                new RoleScope
                {
                    Id = 15,
                    RoleId = "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                    ScopeId = 8
                },
                new RoleScope
                {
                    Id = 16,
                    RoleId = "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                    ScopeId = 9
                },
                new RoleScope
                {
                    Id = 17,
                    RoleId = "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                    ScopeId = 10
                },
                new RoleScope
                {
                    Id = 18,
                    RoleId = "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                    ScopeId = 11
                },
                new RoleScope
                {
                    Id = 19,
                    RoleId = "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                    ScopeId = 12
                },
                new RoleScope
                {
                    Id = 20,
                    RoleId = "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                    ScopeId = 13
                },
                new RoleScope
                {
                    Id = 21,
                    RoleId = "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                    ScopeId = 14
                },
                new RoleScope
                {
                    Id = 22,
                    RoleId = "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                    ScopeId = 15
                },
                new RoleScope
                {
                    Id = 23,
                    RoleId = "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                    ScopeId = 16
                },
                new RoleScope
                {
                    Id = 24,
                    RoleId = "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                    ScopeId = 17
                },
                new RoleScope
                {
                    Id = 25,
                    RoleId = "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                    ScopeId = 18
                },
                new RoleScope
                {
                    Id = 26,
                    RoleId = "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                    ScopeId = 19
                },
                new RoleScope
                {
                    Id = 27,
                    RoleId = "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                    ScopeId = 20
                }
            );

            #endregion

            #region Settings

            _modelBuilder.Entity<Setting>().HasData(
                new Setting() { Id = 1, Key = SettingCode.MaxLoginAttempts.ToString(), DefaultValue = "3", Value = "3", Description = "Número máximo de tentativas de autenticação falhadas", SettingType = (int)SettingsType.USER, CreatedAt = DateTime.Now },
                new Setting() { Id = 2, Key = SettingCode.CaptchaSiteKey.ToString(), DefaultValue = "6Lf2t0sUAAAAABiszBasjJuBZXTdqMy00zOKPOFt", Value = "6Lf2t0sUAAAAABiszBasjJuBZXTdqMy00zOKPOFt", Description = "Site key usada pelo serviço de reCAPTCHA da Google", SettingType = (int)SettingsType.USER, CreatedAt = DateTime.Now },
                new Setting() { Id = 3, Key = SettingCode.CaptchaSecretKey.ToString(), DefaultValue = "6Lf2t0sUAAAAAPwP3kIvpynFqPp-7_QLfQoDQtZd", Value = "6Lf2t0sUAAAAAPwP3kIvpynFqPp-7_QLfQoDQtZd", Description = "Secret key usada pelo serviço de reCAPTCHA da Google", SettingType = (int)SettingsType.USER, CreatedAt = DateTime.Now },
                new Setting() { Id = 4, Key = SettingCode.MailchimpApiKey.ToString(), DefaultValue = "123456", Value = "123456", Description = "API key usada para comunicar com o Mailchimp", SettingType = (int)SettingsType.USER, CreatedAt = DateTime.Now },
                new Setting() { Id = 5, Key = SettingCode.MailchimpListQuantity.ToString(), DefaultValue = "20", Value = "20", Description = "Quantidade máxima de emails por lista", SettingType = (int)SettingsType.USER, CreatedAt = DateTime.Now },
                new Setting() { Id = 6, Key = SettingCode.GeocodeServiceInUse.ToString(), DefaultValue = "HERE", Value = "HERE", Description = "Escolha o serviço 'HERE', 'PositionStack' ou 'Google' - preencha a respectiva API key", SettingType = (int)SettingsType.USER, CreatedAt = DateTime.Now },
                new Setting() { Id = 7, Key = SettingCode.HEREgeocodeApiKey.ToString(), DefaultValue = "123456", Value = "123456", Description = "API key do serviço de geolocalização da 'HERE'", SettingType = (int)SettingsType.USER, CreatedAt = DateTime.Now },
                new Setting() { Id = 8, Key = SettingCode.PositionStackGeocodeApiKey.ToString(), DefaultValue = "123456", Value = "123456", Description = "API key do serviço de geolocalização da 'PositionStack'", SettingType = (int)SettingsType.USER, CreatedAt = DateTime.Now },
                new Setting() { Id = 9, Key = SettingCode.GoogleGeocodeApiKey.ToString(), DefaultValue = "123456", Value = "123456", Description = "API key do serviço de geolocalização da 'Google'", SettingType = (int)SettingsType.USER, CreatedAt = DateTime.Now }
            );

            #endregion


            #region AssociationTypes

            _modelBuilder.Entity<AssociationType>().HasData(
                new AssociationType() { Id = 1, Code = "transition_initiative", Label = "Iniciativa de Transição Local", VatRequired = false, Description = "Identifica que é iniciativa local da transição", CreatedAt = DateTime.Now },
                new AssociationType() { Id = 2, Code = "association_foundation", Label = "Associação/Cooperativa/Fundação", VatRequired = true, Description = "Identifica que é uma associação ou cooperativa ou fundação", CreatedAt = DateTime.Now },
                new AssociationType() { Id = 3, Code = "movement_initiative", Label = "Movimento", VatRequired = false, Description = "Identifica que é um movimento sem entidade formal", CreatedAt = DateTime.Now },
                new AssociationType() { Id = 4, Code = "store", Label = "Comércio Sustentável", VatRequired = true, Description = "Identifica que é um comércio sustentável", CreatedAt = DateTime.Now }
            );

            #endregion

        }
    }
}
