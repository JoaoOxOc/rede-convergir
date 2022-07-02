using UserService.Entities;
using UserService.Models;

namespace UserService.Services.UserManager
{
    public interface IRoleScopeManager
    {
        List<RoleScope> ReplaceRoleScopes(string roleId, List<RoleScope>? roleScopes);

        List<RoleScope> ReplaceRoleScopes(string roleId, List<ScopeModel>? scopes);

        List<RoleScope> ReplaceScopeRoles(int scopeId, List<RoleScope>? scopeRoles);
    }
}
