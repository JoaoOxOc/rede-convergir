using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using UserService.Entities;
using UserService.Models;

namespace UserService.Services.UserManager
{
    public interface ITPUserManager
    {
        Task<User> SearchUser(string username);
        Task<User> SearchUserById(string userId);
        Task<User> ValidateLoginUser(string username, string password);
        Task<IList<string>> GetRolesAsync(User user);
        Task<IdentityResult> RemoveFromRoleAsync(User user, string roleToRemove);
        Task<List<Claim>> GetUserClaims(User user);
        Task<List<Claim>> GetUserClaimsPasswordRecovery(User user);
        Task<List<Claim>> GetAssociationClaimsConfirmEmail(Association association);
        Task<List<Claim>> GetNewsletterSubscriptionClaims();
        Task<IdentityResult> CreateUser(User user, string? password, string? roleName);
        Task<IdentityResult> UpdateUser(User user);
        Task<bool> UpdateUserPassword(string userId, string newPassword);
        Task<IdentityResult> AddUserToRole(User user, string role);
        Task<IdentityResult> CreateRole(User user, string role);
        Task<List<Claim>> GetUserScopes(User user);
        Task<List<User>> GetUsers();
        Task<ProfileModel> GetUserProfileById(string userId);

        Task<KeyValuePair<User, Association>> CreateUserWithAssociation(User user, Association association, string password);
        Task<Association> UpdateAssociation(Association association);
        Task<Association> SearchAssociationByEmail(string email);
        Task<Association> SearchAssociationByVat(string vat);
        Task<Association> SearchAssociationById(int associationId);
    }
}
