using CommonLibrary.Enums;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using UserService.Services.Database;

namespace UserService.Services.UserManager
{
    public class UserRoleManager : IUserRoleManager
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _uow;

        public UserRoleManager(RoleManager<IdentityRole> roleManager, IUnitOfWork uow)
        {
            _roleManager = roleManager;
            _uow = uow;
        }

        public List<IdentityRole> FindRolesByPartialName(string partialName, bool shouldContain = true)
        {
            Expression<Func<IdentityRole, bool>> filterRole = (x => shouldContain ? x.Name.ToLower().Contains(partialName.ToLower()) : !x.Name.ToLower().Contains(partialName.ToLower()));
            return _uow.IdentityRoleRepository.Get(null, null, filterRole, "Name", SortDirection.Ascending);
        }

        public List<string> GetUserIdsByRole(string? role)
        {
            List<string> userIds = new List<string>();
            List<string> roleIds = null;
            switch (role)
            {
                case "System":
                    {
                        var roles = FindRolesByPartialName("association", false);
                        roleIds = roles != null ? roles.Select(x => x.Id).ToList() : new List<string>();
                    }
                    break;
                case "Customer":
                    {
                        var roles = FindRolesByPartialName("association");
                        roleIds = roles != null ? roles.Select(x => x.Id).ToList() : new List<string>();
                    }
                    break;
            }
            if (roleIds != null && roleIds.Count > 0)
            {
                Expression<Func<IdentityUserRole<string>, bool>> filter = (x => roleIds.Contains(x.RoleId));
                var userRoles = _uow.IdentityUserRoleRepository.Get(null, null, filter, "RoleId", SortDirection.Ascending);
                userIds = userRoles != null ? userRoles.Select(x => x.UserId).ToList() : new List<string>();
            }
            return userIds;
        }
        public IdentityRole GetUserRoleByUserId(string userId)
        {
            IdentityRole role = null;
            if (!string.IsNullOrEmpty(userId))
            {
                Expression<Func<IdentityUserRole<string>, bool>> filter = (x => x.UserId == userId);
                var userRole = _uow.IdentityUserRoleRepository.Get(null, null, filter, "RoleId", SortDirection.Ascending).FirstOrDefault();
                if (userRole != null)
                {
                    Expression<Func<IdentityRole, bool>> filterRole = (x => x.Id == userRole.RoleId);
                    role = _uow.IdentityRoleRepository.Get(null, null, filterRole, "Name", SortDirection.Ascending).FirstOrDefault();
                }
            }
            return role;
        }
    }
}
