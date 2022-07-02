using CommonLibrary.Enums;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using UserService.Entities;
using UserService.Models;
using UserService.Services.Database;

namespace UserService.Services.UserManager
{
    public class RoleScopeManager : IRoleScopeManager
    {
        private readonly IUnitOfWork _uow;
        public RoleScopeManager(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public List<RoleScope> ReplaceRoleScopes(string roleId, List<RoleScope>? roleScopes)
        {
            Expression<Func<RoleScope, bool>> scopeFilter = (x => x.RoleId == roleId);
            if (roleScopes != null && roleScopes.Count > 0)
            {
                var currentScopes = _uow.RoleScopeRepository.Get(null, null, scopeFilter, "Id", SortDirection.Ascending, string.Empty);
                if (currentScopes != null && currentScopes.Count > 0)
                {
                    _uow.RoleScopeRepository.Delete(currentScopes);
                    _uow.Save();
                }

                _uow.RoleScopeRepository.Add(roleScopes);
                _uow.Save();
            }
            return _uow.RoleScopeRepository.Get(null, null, scopeFilter, "Scope.ScopeName", SortDirection.Ascending, "Scope");
        }

        public List<RoleScope> ReplaceRoleScopes(string roleId, List<ScopeModel>? scopes)
        {
            List<RoleScope> roleScopes = new List<RoleScope>();

            if (scopes != null)
            {
                foreach (var scope in scopes)
                {
                    if (scope.ScopeId.HasValue)
                    {
                        roleScopes.Add(new RoleScope
                        {
                            RoleId = roleId,
                            ScopeId = scope.ScopeId.Value
                        });
                    }
                }

                roleScopes = ReplaceRoleScopes(roleId, roleScopes);
            }

            return roleScopes;
        }

        public List<RoleScope> ReplaceScopeRoles(int scopeId, List<RoleScope>? scopeRoles)
        {
            Expression<Func<RoleScope, bool>> scopeFilter = (x => x.ScopeId == scopeId);
            if (scopeRoles != null && scopeRoles.Count > 0)
            {
                var currentScopes = _uow.RoleScopeRepository.Get(null, null, scopeFilter, "Id", SortDirection.Ascending, string.Empty);
                if (currentScopes != null && currentScopes.Count > 0)
                {
                    _uow.RoleScopeRepository.Delete(currentScopes);
                    _uow.Save();
                }

                _uow.RoleScopeRepository.Add(scopeRoles);
                _uow.Save();
            }
            return _uow.RoleScopeRepository.Get(null, null, scopeFilter, "Scope.ScopeName", SortDirection.Ascending, "Scope");
        }
    }
}
