using CommonLibrary.Entities;
using CommonLibrary.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Entities;
using UserService.Enum;
using UserService.Services.Database;
using UserService.Services.UserManager;

namespace UserService.Migrations.Config
{
    public class DbInitializer
    {
        private readonly ITokenManager _tokenManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;

        public DbInitializer(ITokenManager tokenManager, RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            this._tokenManager = tokenManager;
            this._roleManager = roleManager;
            this._userManager = userManager;
        }

        /// <summary>
        /// Insert data into database on runtime
        /// 
        /// Prevents the existing of sensitive data in migration folder, like tokens
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(DatabaseContext context)
        {
            #region Client Credentials

            var clientCredentials = new ClientCredential[]
            {
                new ClientCredential { Name = "tpbackoffice", ClientId= "tpbackoffice", Description="transição portugal backoffice", ClientSecret = _tokenManager.GetClientToken().Token },
                new ClientCredential { Name = "tphome", ClientId = "tphome", Description = "transição portugal homepage", ClientSecret = _tokenManager.GetClientToken().Token },
                new ClientCredential { Name = "tp single sign on", ClientId = "tp_sso", Description = "transição portugal login/registo", ClientSecret = _tokenManager.GetClientToken().Token }
            };

            foreach (ClientCredential obj in clientCredentials)
            {
                if (context.ClientCredential.Where(x => x.ClientId == obj.ClientId).FirstOrDefault() == null)
                {
                    context.ClientCredential.Add(obj);
                }
            }

            #endregion


            #region Users

            var users = new User[]
            {
                new User { Name = "Administrator", UserName = "admin", NormalizedUserName = "ADMIN", Email = "admin@tp.int", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"), IsVerified = true, IsActive = true, IsEmailVerified = true, EmailConfirmed = true, Timezone = "UTC", LangCode = "pt-pt", CreatedAt = DateTime.Now }
            };

            foreach (User obj in users)
            {
                if (context.User.Where(x => x.UserName == obj.UserName).FirstOrDefault() == null)
                {
                    context.User.Add(obj);
                }
                //if (!context.User.Where(x => x.UserName == obj.UserName).Any())
                //{
                //}
            }

            #endregion

            context.SaveChanges();

            #region Users Roles

            if (!_roleManager.RoleExistsAsync("Admin").Result)
            {
                var creationResult = _roleManager.CreateAsync(new IdentityRole("Admin")).Result;
            }

            User admin = context.User.Where(x => x.UserName == "admin").First();

            if (admin != null)
            {
                var creationResult = _userManager.AddToRoleAsync(admin, "Admin").Result;
            }

            #endregion
        }
    }
}
