using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace tpGateway.Services
{
    public class TokenManager : ITokenManager
    {
        private readonly IConfiguration _configuration;
        public TokenManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> ValidateClient(string clientId, string clientToken)
        {
            if (!string.IsNullOrEmpty(clientToken))
            {
                HttpClient client = new HttpClient();
                var json = JsonExtensions.SerializeToJson(new { ClientId = clientId, ClientSecret = clientToken });
                var response = await client.PostAsync(_configuration["JWT:RemoteClientVerification"], new StringContent(json, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();

                return true;
                //using RSA rsa = RSA.Create();
                //rsa.ImportRSAPublicKey(Convert.FromBase64String(_configuration["JWT:SecretPublicKey"]), out _);
                //SecurityToken validatedToken;

                //var validationParameters = new TokenValidationParameters()
                //{
                //    ValidateAudience = false,
                //    ValidateIssuer = true,
                //    ValidateLifetime = false,
                //    IssuerSigningKey = new RsaSecurityKey(rsa),
                //    ValidIssuer = _configuration["JWT:ValidIssuer"],
                //    CryptoProviderFactory = new CryptoProviderFactory()
                //    {
                //        CacheSignatureProviders = false
                //    }
                //};
                //new JwtSecurityTokenHandler().ValidateToken(clientToken, validationParameters, out validatedToken);
                //if (validatedToken != null)
                //{
                //    return true;
                //}
                //else
                //{
                //    return false;
                //}
            }
            else
            {
                return false;
            }
        }
    }
}
