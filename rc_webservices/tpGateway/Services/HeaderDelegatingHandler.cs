using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace tpGateway.Services
{
    public class HeaderDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        ILogger<HeaderDelegatingHandler> _logger;

        public HeaderDelegatingHandler(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<HeaderDelegatingHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// The logic inside sets the request header for the subsequent services that will be called from the gateway, accordingly to routes
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            IEnumerable<string> headerValues;
            bool exists = request.Headers.TryGetValues("Authorization", out headerValues);
            if (exists)
            {
                string accessToken = headerValues.First();

                string[] claims = new string[] { "userId", "sub", System.Security.Claims.ClaimTypes.Role, "scope", "associationId" };

                try
                {
                    List<JwtClaim> userClaims = JwtHelper.ValidateToken(accessToken, _configuration["JWT:ValidAudience"], _configuration["JWT:ValidIssuer"], _configuration["JWT:SecretPublicKey"], claims);
                    if (userClaims != null && userClaims.Count > 0)
                    {
                        _logger.LogInformation("route client app header user id: " + userClaims.Where(x => x.Claim == "userId").Single().Value);
                        request.Headers.Add("UserId", userClaims.Where(x => x.Claim == "userId").Single().Value);
                        request.Headers.Add("AssociationId", userClaims.Where(x => x.Claim == "associationId").FirstOrDefault().Value);
                        request.Headers.Add("UserRole", userClaims.Where(x => x.Claim == System.Security.Claims.ClaimTypes.Role).Single().Value);
                        request.Headers.Add("UserClaims", userClaims.Where(x => x.Claim == "scope").Single().Value);
                    }
                }
                catch (System.Exception) { }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
