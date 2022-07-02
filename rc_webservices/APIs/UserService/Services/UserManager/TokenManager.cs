using CommonLibrary.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserService.Models;

namespace UserService.Services.UserManager
{
    public class TokenManager : ITokenManager
    {

        private readonly IConfiguration _configuration;

        public TokenManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public JwtResponse GetToken(List<Claim> authClaims, int? expireTimeMinutes, string? fingerprint)
        {
            var privateKey = Convert.FromBase64String(_configuration["JWT:SecretPrivateKey"]);

            using RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(privateKey, out _);

            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
            };

            if (!string.IsNullOrEmpty(fingerprint))
            {
                UTF8Encoding encoding = new UTF8Encoding();
                authClaims.Add(new Claim("userContext", Convert.ToBase64String(rsa.Encrypt(encoding.GetBytes(fingerprint), RSAEncryptionPadding.Pkcs1))));
            }

            //var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            int tokenValidityInMinutes = 0;
            if (expireTimeMinutes.HasValue)
            {
                tokenValidityInMinutes = expireTimeMinutes.Value;
            }
            else
            {
                _ = int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out tokenValidityInMinutes);
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: signingCredentials//new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return new JwtResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = new DateTimeOffset(DateTime.Now.AddMinutes(tokenValidityInMinutes)).ToUnixTimeSeconds(),
            };
        }

        public JwtResponse GetClientToken()
        {
            var privateKey = Convert.FromBase64String(_configuration["JWT:SecretPrivateKey"]);

            using RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(privateKey, out _);

            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                claims: new List<Claim>() { new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) }, 
                signingCredentials: signingCredentials
                );

            return new JwtResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };
        }

        public JwtResponse GetClientToken(List<Claim> claims)
        {
            var privateKey = Convert.FromBase64String(_configuration["JWT:SecretPrivateKey"]);

            using RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(privateKey, out _);

            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                claims: claims,
                signingCredentials: signingCredentials
                );

            return new JwtResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };
        }

        /// <summary>
        /// read: https://github.com/cornflourblue/dotnet-6-jwt-refresh-tokens-api/blob/master/Controllers/UsersController.cs
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public KeyValuePair<string,int> GenerateRefreshToken(string userId, string? fingerprint)
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

            var privateKey = Convert.FromBase64String(_configuration["JWT:SecretPrivateKey"]);

            using RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(privateKey, out _);

            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
            };

            var claims = new List<Claim>() { new Claim("userId", userId) };
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            if (!string.IsNullOrEmpty(fingerprint))
            {
                UTF8Encoding encoding = new UTF8Encoding();
                claims.Add(new Claim("userContext", Convert.ToBase64String(rsa.Encrypt(encoding.GetBytes(fingerprint), RSAEncryptionPadding.Pkcs1))));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.Now.AddDays(refreshTokenValidityInDays),
                signingCredentials: signingCredentials
                );

            //return new JwtResponse
            //{
            //    Token = new JwtSecurityTokenHandler().WriteToken(token),
            //    ExpiresAt = refreshTokenValidityInDays,
            //};

            return new KeyValuePair<string, int>(new JwtSecurityTokenHandler().WriteToken(token), refreshTokenValidityInDays);
        }

        public AuthCookieInfo GenerateAuthFingerprint(string cookieName)
        {
            AuthCookieInfo cookie = new AuthCookieInfo();
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);
            cookie.CookieProperties = new CookieOptions
            {
                Domain = _configuration["ApplicationSettings:CookieDomain"],
                Expires = DateTime.Now.AddDays(refreshTokenValidityInDays),
                IsEssential = true,
                // Set the secure flag, which Chrome's changes will require for SameSite none.
                // Note this will also require you to be running on HTTPS.
                Secure = true,

                // Set the cookie to HTTP only which is good practice unless you really do need
                // to access it client side in scripts.
                HttpOnly = true,

                // Add the SameSite attribute, this will emit the attribute with a value of none.
                // To not emit the attribute at all set
                // SameSite = (SameSiteMode)(-1)
                SameSite = _configuration["ApplicationSettings:CookiePolicy"] == "None" ? SameSiteMode.None : (_configuration["ApplicationSettings:CookiePolicy"] == "Strict" ? SameSiteMode.Strict : SameSiteMode.Lax)
            };
            cookie.CookieName = cookieName;
            cookie.CookieValue = StringHelper.GenerateRandomString(32);

            return cookie;
        }

        public bool ValidateAuthFingerprint(string cookieValue, string userContext)
        {
            if (!string.IsNullOrEmpty(cookieValue) && !string.IsNullOrEmpty(userContext))
            {
                using RSA rsa = RSA.Create();
                //rsa.ImportRSAPublicKey(Convert.FromBase64String(_configuration["JWT:SecretPublicKey"]), out _);
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(_configuration["JWT:SecretPrivateKey"]), out _);

                var decrypted = rsa.Decrypt(Convert.FromBase64String(userContext), RSAEncryptionPadding.Pkcs1);

                UTF8Encoding encoding = new UTF8Encoding();
                return cookieValue == encoding.GetString(decrypted, 0, decrypted.Length);
            }
            else
            {
                return false;
            }
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            using RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(_configuration["JWT:SecretPublicKey"]), out _);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
                IssuerSigningKey = new RsaSecurityKey(rsa),
                CryptoProviderFactory = new CryptoProviderFactory()
                {
                    CacheSignatureProviders = false
                },
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;

        }

        public ClaimsPrincipal? GetPrincipalFromRefreshToken(string? token)
        {
            using RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(_configuration["JWT:SecretPublicKey"]), out _);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new RsaSecurityKey(rsa),
                CryptoProviderFactory = new CryptoProviderFactory()
                {
                    CacheSignatureProviders = false
                },
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;

        }
    }
}
