using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonLibrary.Extensions
{
    public class PermissionsHelper
    {
        /// <summary>
        /// Get the role from JWT claim
        /// </summary>
        /// <param name="userClaims">the user token extracted claims</param>
        /// <returns>the user role, or string empty case it doesn't exist</returns>
        public static string GetUserRoleFromClaim(List<JwtClaim> userClaims)
        {
            string userRole = string.Empty;
            if (userClaims != null && userClaims.Count > 0)
            {
                var roleClaim = userClaims.Where(x => x.Claim == System.Security.Claims.ClaimTypes.Role).SingleOrDefault();
                if (roleClaim != null)
                {
                    userRole = roleClaim.Value;
                }
            }
            return userRole;
        }

        /// <summary>
        /// Validates if the Role claim value is inside the permitted roles list
        /// </summary>
        /// <param name="userClaims">the user token extracted claims</param>
        /// <param name="permittedRoles">permitted roles list</param>
        /// <returns>true if user claim role is inside permitted roles list, false otherwise</returns>
        public static bool ValidateRoleClaimPermission(List<JwtClaim> userClaims, List<string> permittedRoles)
        {
            if (permittedRoles != null && permittedRoles.Count > 0 && userClaims != null && userClaims.Count > 0)
            {
                return permittedRoles.Contains(userClaims.Where(x => x.Claim == System.Security.Claims.ClaimTypes.Role).Single().Value);
            }
            else if (permittedRoles == null || permittedRoles.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Validates if the Role claim value is inside the permitted roles list
        /// </summary>
        /// <param name="userRole">the user role</param>
        /// <param name="permittedRoles">permitted roles list</param>
        /// <returns>true if user claim role is inside permitted roles list, false otherwise</returns>
        public static bool ValidateRoleClaimPermission(string userRole, List<string> permittedRoles)
        {
            if (permittedRoles != null && permittedRoles.Count > 0 && !string.IsNullOrEmpty(userRole))
            {
                return permittedRoles.Contains(userRole);
            }
            else if (permittedRoles == null || permittedRoles.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Validates if the user as any scope defined for the action
        /// </summary>
        /// <param name="userScopes"> the list of user scopes registered on his token</param>
        /// <param name="permittedScopes">the list of permitted scopes on a specific action</param>
        /// <returns></returns>
        public static bool ValidateUserScopesPermissionAny(List<string>? userScopes, List<string> permittedScopes)
        {
            if (permittedScopes != null && permittedScopes.Count > 0 && userScopes != null)
            {
                return permittedScopes.Select(x => x)
                          .Intersect(userScopes)
                          .Any();
            }
            else if (permittedScopes == null || permittedScopes.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Validates if the user as any scope defined for the action
        /// </summary>
        /// <param name="userScopes"> the list of user scopes registered on his token</param>
        /// <param name="permittedScopes">the list of permitted scopes on a specific action</param>
        /// <returns></returns>
        public static bool ValidateUserScopesPermissionAll(List<string>? userScopes, List<string> permittedScopes)
        {
            if (permittedScopes != null && permittedScopes.Count > 0 && userScopes != null)
            {
                return permittedScopes.Select(x => x)
                          .Intersect(userScopes).Count() == permittedScopes.Count;
            }
            else if (permittedScopes == null || permittedScopes.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
