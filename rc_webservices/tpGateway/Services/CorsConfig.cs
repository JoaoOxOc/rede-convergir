using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace tpGateway.Services
{
    public static class CorsConfig
    {
        public static IServiceCollection ConfigureCors(this IServiceCollection services, IConfiguration configuration)
        {
            string cors = configuration.GetSection("ApplicationSettings:Cors").Get<string>();
            string[] corsUrl = !string.IsNullOrEmpty(cors) ? cors.Split(',') : null;
            string exposedHeadersString = configuration.GetSection("ApplicationSettings:ExposedHeaders").Get<string>();
            string[] exposedHeaders = !string.IsNullOrEmpty(exposedHeadersString) ? exposedHeadersString.Split(',') : System.Array.Empty<string>();

            if (corsUrl != null)
            {
                services.AddCors(options =>
                {
                    options.AddPolicy("TpCorsPolicy",
                        builder => builder
                            .WithOrigins(corsUrl) // or AllowAnyOrigin() combined with the next commented line
                                                  //.SetIsOriginAllowed(origin => true) // allow any origin
                            .WithExposedHeaders(exposedHeaders)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials());
                });
            }

            return services;
        }
    }
}
