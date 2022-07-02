using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CommonLibrary.Extensions
{
    public class JwtClaim
    {
        public JwtClaim(string claim, string value)
        {
            this.Claim = claim;
            this.Value = value;
        }

        public string Claim { get; set; }
        public string Value { get; set; }
    }

    public class JwtHelper
    {
        public static List<JwtClaim> ValidateToken(string httpHeaderToken, string audience, string issuer, string secret, string[] claims)
        {
            List<JwtClaim> result = null;

            var bearer = httpHeaderToken.Split(' ')[1];

            using RSA rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(secret), out _);
            SecurityToken validatedToken;

            var validationParameters = new TokenValidationParameters()
            {
                ValidAudience = audience,
                ValidIssuer = issuer,
                IssuerSigningKey = new RsaSecurityKey(rsa),
                CryptoProviderFactory = new CryptoProviderFactory()
                {
                    CacheSignatureProviders = false
                }
            };
            new JwtSecurityTokenHandler().ValidateToken(bearer, validationParameters, out validatedToken);
            if (validatedToken != null)
            {
                result = Decode(httpHeaderToken, claims);
            }
            return result;
        }

        public static List<JwtClaim> Decode(string header, string[] args)
        {
            var bearer = header.Split(' ')[1];

            var tokenDecoder = new JwtSecurityTokenHandler();
            var token = (JwtSecurityToken)tokenDecoder.ReadToken(bearer);

            List<JwtClaim> result = new List<JwtClaim>();

            foreach (string obj in args)
            {
                var claimsByType = token.Claims.Where(x => x.Type == obj).Select(c => c.Value);
                result.Add(new JwtClaim(obj, claimsByType.Count() > 1 ? JsonExtensions.SerializeToJson(claimsByType) : claimsByType.FirstOrDefault()));
            }

            return result;
        }

        public static Dictionary<string, string> GetTokenValues(string header, string[] claimsIdentifications)
        {
            Dictionary<string, string> mappedClaimValues = new Dictionary<string, string>();

            List<JwtClaim> jwtClaims = Decode(header, claimsIdentifications);

            try
            {
                foreach (string claimId in claimsIdentifications)
                {
                    var claim = jwtClaims.Where(x => x.Claim == claimId).FirstOrDefault();
                    mappedClaimValues.Add(claimId, claim != null ? claim.Claim : null);
                }
            }
            catch
            {

            }

            return mappedClaimValues;
        }
    }
}
