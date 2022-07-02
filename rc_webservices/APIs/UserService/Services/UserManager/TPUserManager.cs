using CommonLibrary.Enums;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using UserService.Entities;
using UserService.Helpers;
using UserService.Models;
using UserService.Services.Database;
using MicroservicesLibrary.Exceptions;
using CommonLibrary.Extensions;

namespace UserService.Services.UserManager
{
    public class TPUserManager : ITPUserManager
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _uow;

        public TPUserManager(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork uow)
        {
            this._roleManager = roleManager;
            this._uow = uow;
            _userManager = userManager;
        }

        public async Task<User> SearchUser(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(username);
                if (user == null)
                {
                    user = _uow.UserRepository.Get(null, null, (x => x.UserName == username || x.Email == username), "Id", SortDirection.Ascending, "").SingleOrDefault();
                }
            }

            return user;
        }

        public async Task<User> SearchUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                user = _uow.UserRepository.Get(null, null, (x => x.Id == userId), "Id", SortDirection.Ascending, "").SingleOrDefault();
            }

            return user;
        }

        public async Task<User> ValidateLoginUser(string username, string password)
        {
            var user = await this.SearchUser(username);
            if (user != null)
            {
                if(await _userManager.CheckPasswordAsync(user, password))
                {
                    if(user.IsVerified && user.IsActive)
                    {
                        return user;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<IList<string>> GetRolesAsync(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<IdentityResult> RemoveFromRoleAsync(User user, string roleToRemove)
        {
            return await _userManager.RemoveFromRoleAsync(user, roleToRemove);
        }

        public async Task<List<Claim>> GetUserClaims(User user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim("userId", user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            if (user.AssociationId.HasValue)
            {
                authClaims.Add(new Claim("associationId", user.AssociationId.ToString()));
            }

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            return authClaims;
        }

        public async Task<List<Claim>> GetUserClaimsPasswordRecovery(User user)
        {
            var authClaims = new List<Claim>
                {
                    new Claim("userId", user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            return authClaims;
        }

        public async Task<List<Claim>> GetAssociationClaimsConfirmEmail(Association association)
        {
            var authClaims = new List<Claim>
                {
                    new Claim("associationId", association.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            return authClaims;
        }

        public async Task<List<Claim>> GetNewsletterSubscriptionClaims()
        {
            var authClaims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            return authClaims;
        }

        public async Task<List<Claim>> GetUserScopes(User user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            List<IdentityRole> userIdentityRoles = _uow.IdentityRoleRepository.Get(null, null, (x => userRoles.Contains(x.Name)), "Id", SortDirection.Ascending, "");
            var userRoleScopes = _uow.RoleScopeRepository.Get(null, null, (x => userIdentityRoles.Select(x => x.Id).ToList().Contains(x.RoleId)), "Id", SortDirection.Ascending, "");
            var userScopes = _uow.ScopeRepository.Get(null, null, (x => userRoleScopes.Select(x => x.ScopeId).ToList().Contains(x.Id)), "Id", SortDirection.Ascending, "");

            var authClaims = new List<Claim>();
            //authClaims.Add(new Claim("scope", userScopes.Select(x=>x.ScopeName).Aggregate((a,b)=> a + "," + b )));
            authClaims.Add(new Claim("scope", JsonSerializer.Serialize(userScopes.Select(x => x.ScopeName)), JsonClaimValueTypes.JsonArray));

            return authClaims;
        }

        public async Task<IdentityResult> CreateUser(User user, string? password, string? roleName)
        {
            IdentityResult creationResult;
            var validMessage = await this.ValidatePassword(password);
            if (validMessage == "Succeeded")
            {
                var newPasswordHashed = BCrypt.Net.BCrypt.HashPassword(password);
                creationResult = await _userManager.CreateAsync(user, newPasswordHashed);
                if (creationResult.Succeeded)
                {
                    var roleResult = await this.AddUserToRole(user, roleName);
                    if (!roleResult.Succeeded)
                    {
                        throw new AppException(JsonSerializer.Serialize(roleResult.Errors));
                    }
                }
                else
                {
                    throw new AppException(JsonSerializer.Serialize(creationResult.Errors));
                }
            }
            else
            {
                throw new AuthException(validMessage);
            }

            return creationResult;
        }

        public async Task<KeyValuePair<User, Association>> CreateUserWithAssociation(User user, Association association, string password)
        {
            KeyValuePair<User, Association> creationData = new KeyValuePair<User, Association>();
            var newPasswordHashed = BCrypt.Net.BCrypt.HashPassword(password);
            var validMessage = await this.ValidatePassword(password);
            if (validMessage == "Succeeded")
            {
                association.CanonicalNameAlias = association.Name.ToLower().Replace(" ", "-");
                association.CanonicalNameAlias = StringHelper.RemoveAccents(association.CanonicalNameAlias);
                _uow.AssociationRepository.Add(association);
                _uow.Save();

                user.AssociationId = association.Id;
                var creationResult = await _userManager.CreateAsync(user);
                if (creationResult.Succeeded)
                {
                    var result = await _userManager.AddPasswordAsync(user, newPasswordHashed);
                    if (result.Succeeded)
                    {
                        var roleResult = await this.AddUserToRole(user, "AssociationAdmin");
                        if (roleResult.Succeeded)
                        {
                            //user.PasswordHash = newPasswordHashed;
                            //_uow.UserRepository.Add(user);
                            //_uow.Save();

                            creationData = new KeyValuePair<User, Association>(user, association);
                        }
                        else
                        {
                            throw new AppException(JsonSerializer.Serialize(roleResult.Errors));
                        }
                    }
                    else
                    {
                        throw new AppException(JsonSerializer.Serialize(result.Errors));
                    }
                }
                else
                {
                    throw new AppException(JsonSerializer.Serialize(creationResult.Errors));
                }
            }
            else
            {
                throw new AuthException(validMessage);
            }

            return creationData;
        }

        public async Task<IdentityResult> UpdateUser(User user)
        {
            var creationResult = await _userManager.UpdateAsync(user);

            return creationResult;
        }

        public async Task<Association> UpdateAssociation(Association association)
        {
            association.UpdatedAt = DateTime.Now;
            _uow.AssociationRepository.Update(association);
            _uow.Save();

            return association;
        }

        public async Task<bool> UpdateUserPassword(string userId, string newPassword)
        {
            var user = _uow.UserRepository.Get(null, null, (x => x.Id == userId), "UserName", SortDirection.Ascending).FirstOrDefault();
            if (user != null)
            {
                var validMessage = await this.ValidatePassword(newPassword);
                if (validMessage == "Succeeded")
                {
                    var newPasswordHashed = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    var result = await _userManager.RemovePasswordAsync(user);
                    if (result.Succeeded)
                    {
                        var result2 = await _userManager.AddPasswordAsync(user, newPasswordHashed);
                        if (result2.Succeeded)
                        {
                            user.PasswordHash = newPasswordHashed;
                            user.UpdatedAt = DateTime.Now;
                            _uow.UserRepository.Update(user);
                            _uow.Save();
                            return true;
                        }
                        else
                        {
                            throw new AuthException(JsonSerializer.Serialize(result2.Errors));
                        }
                    }
                    else
                    {
                        throw new AuthException(JsonSerializer.Serialize(result.Errors));
                    }
                }
                else
                {
                    throw new AuthException(validMessage);
                }
            }
            return false;
        }

        private async Task<bool> ValidateRoleExists(string? roleName)
        {
            var roleExists = false;

            if (await _roleManager.RoleExistsAsync(roleName))
            {
                roleExists = true;
            }
            else
            {
                var roleEntity = _uow.IdentityRoleRepository.Get(null, null, (x => x.Name == roleName), "Name").FirstOrDefault();
                if (roleEntity != null)
                {
                    roleExists = true;
                }
            }

            return roleExists;
        }

        public async Task<IdentityResult> AddUserToRole(User user, string? role)
        {
            var creationResult = new IdentityResult();

            if (await ValidateRoleExists(role))
            {
                try
                {
                    creationResult = await _userManager.AddToRoleAsync(user, role);
                }
                catch(Exception ex)
                {
                    throw new AppException(ex.InnerException != null ? ex.InnerException.Message : "" + " Exception: " + ex.Message);
                }
            }
            return creationResult;
        }

        public async Task<IdentityResult> CreateRole(User user, string role)
        {
            var creationResult = new IdentityResult();
            if (!await ValidateRoleExists(role))
            {
                creationResult = await _roleManager.CreateAsync(new IdentityRole(role));
            }
            return creationResult;
        }

        public async Task<List<User>> GetUsers()
        {
            var users = new List<User>();

            users = _uow.UserRepository.Get(null, null, null, "UserName", SortDirection.Ascending);

            return users;
        }

        public async Task<ProfileModel> GetUserProfileById(string userId)
        {
            var user = _uow.UserRepository.Get(null, null, (x => x.Id == userId), "UserName", SortDirection.Ascending, "Association").FirstOrDefault();
            if (user != null)
            {
                return new ProfileModel
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Name = user.Name,
                    Phone = user.PhoneNumber,
                    IsActive = user.IsActive,
                    IsEmailVerified = user.IsEmailVerified,
                    IsVerified = user.IsVerified,
                    AssociationId = user.AssociationId,
                    AssociationLogoImage = user.Association?.LogoImage,
                    AssociationName = user.Association?.Name,
                    //JobTitle = "Lead Developer",
                    //UserRole = "Admin",
                    //Posts = 27,
                    //Location = "San Francisco, USA",
                    //Avatar = "/static/images/avatars/3.jpg",
                    //CoverImg = "/static/images/placeholders/covers/5.jpg",
                    //Followers = 6513,
                    //Description = "Curabitur at ipsum ac tellus semper interdum."
                };
            }
            return new ProfileModel();
        }

        public async Task<Association> SearchAssociationByEmail(string email)
        {
            var association = _uow.AssociationRepository.Get(null, null, (x => x.Email == email), "Name", SortDirection.Ascending).FirstOrDefault();

            return association;
        }

        public async Task<Association> SearchAssociationByVat(string vat)
        {
            var association = _uow.AssociationRepository.Get(null, null, (x => x.Vat == vat), "Name", SortDirection.Ascending).FirstOrDefault();

            return association;
        }

        public async Task<Association> SearchAssociationById(int associationId)
        {
            var association = _uow.AssociationRepository.Get(null, null, (x => x.Id == associationId), "Name", SortDirection.Ascending).FirstOrDefault();

            return association;
        }

        private async Task<string> ValidatePassword(string? password)
        {
            var passwordValidator = new PasswordValidator<User>();
            var result = await passwordValidator.ValidateAsync(_userManager, null, password);
            if(result.Succeeded)
            {
                return "Succeeded";
            }
            else
            {
                return JsonSerializer.Serialize(result.Errors);
            }
        }
    }
}
