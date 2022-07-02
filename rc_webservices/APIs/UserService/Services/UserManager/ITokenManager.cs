using System.Security.Claims;
using UserService.Models;

namespace UserService.Services.UserManager
{
    public interface ITokenManager
    {
        AuthCookieInfo GenerateAuthFingerprint(string cookieName);
        bool ValidateAuthFingerprint(string cookieValue, string userContext);
        JwtResponse GetToken(List<Claim> authClaims, int? expireTimeMinutes, string? fingerprint);
        JwtResponse GetClientToken();
        JwtResponse GetClientToken(List<Claim> claims);
        KeyValuePair<string, int> GenerateRefreshToken(string userId, string? fingerprint);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);
        ClaimsPrincipal? GetPrincipalFromRefreshToken(string? token);
    }
}
