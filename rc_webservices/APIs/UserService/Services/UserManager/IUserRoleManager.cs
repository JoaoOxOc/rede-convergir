using Microsoft.AspNetCore.Identity;

namespace UserService.Services.UserManager
{
    public interface IUserRoleManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="partialName"></param>
        /// <param name="shouldContain">if the role should contain the partial name or not; default is true</param>
        /// <returns></returns>
        List<IdentityRole> FindRolesByPartialName(string partialName, bool shouldContain = true);

        List<string> GetUserIdsByRole(string? role);
        IdentityRole GetUserRoleByUserId(string userId);
    }
}
