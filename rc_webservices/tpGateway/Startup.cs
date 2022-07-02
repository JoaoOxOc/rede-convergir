using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.Administration;
using Ocelot.Authorization;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using tpGateway.Services;
using MicroservicesLibrary.OpenApi;

namespace tpGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureCors(Configuration);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Transition Portugal Gateway API",
                    Description = "Gateway for all TP Web Services",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Example Contact",
                        Url = new Uri("https://example.com/contact")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Example License",
                        Url = new Uri("https://example.com/license")
                    }
                });
                c.OperationFilter<RequiredHeaderParameters>();
            });

            /*
             * Configure validation of JWT signed with a private asymmetric key.
             * 
             * We'll use a public key to validate if the token was signed
             * with the corresponding private key.
             */
            services.AddSingleton<RsaSecurityKey>(provider => {
                // It's required to register the RSA key with depedency injection.
                // If you don't do this, the RSA instance will be prematurely disposed.

                RSA rsa = RSA.Create();
                rsa.ImportRSAPublicKey(
                    source: Convert.FromBase64String(Configuration["JWT:SecretPublicKey"]),
                    bytesRead: out int _
                );

                return new RsaSecurityKey(rsa);
            });
            services.TryAddSingleton<ITokenManager,TokenManager>();

            // Adding Authentication
            services.AddAuthentication()
            // Adding Jwt Bearer
            .AddJwtBearer("TpAuthKey", options =>
            {
                SecurityKey rsa = services.BuildServiceProvider().GetRequiredService<RsaSecurityKey>();
                options.IncludeErrorDetails = true; // <- great for debugging
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.Zero,

                    ValidAudience = Configuration["JWT:ValidAudience"],
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                    IssuerSigningKey = rsa,
                    //CryptoProviderFactory = new CryptoProviderFactory()
                    //{
                    //    CacheSignatureProviders = false
                    //}
                };
            });
            // TODO: apply the correct secret
            services.AddOcelot(Configuration).AddDelegatingHandler<HeaderDelegatingHandler>(true).AddAdministration("/administration", "secret");

            services.AddSwaggerForOcelot(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseRouting();

            // cors policy
            app.UseCors("TpCorsPolicy");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var configuration = new OcelotPipelineConfiguration
            {
                //Implementing a custom client credentials check, since I'm not using IdentityServer
                PreAuthorizationMiddleware = async (ctx, next) =>
                {
                    var downstreamRoute = ctx.Items.DownstreamRoute();
                    string clientId = ctx.Request.Headers["ClientId"].ToString();
                    string clientToken = ctx.Request.Headers["ClientAuthorization"].ToString();

                    var tokenManager = app.ApplicationServices.GetRequiredService<ITokenManager>();
                    var validated = await tokenManager.ValidateClient(clientId, clientToken);
                    if (!validated)
                    {
                        ctx.Items.SetError(new UnauthorizedError("client_not_authorized"));
                    }
                    else
                    {
                        logger.LogInformation("route client app is authorized: " + validated);
                    }

                    await next.Invoke();
                }
            };
            //    PreQueryStringBuilderMiddleware = async (ctx, next) =>
            //    {
            //        await next.Invoke();
            //    },
            //    AuthenticationMiddleware = async (ctx, next) =>
            //    {
            //        string token = ctx.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            //        var downstreamRoute = ctx.Items.DownstreamRoute();

            //        logger.LogInformation("route is authenticated " + downstreamRoute.AuthenticationOptions.AuthenticationProviderKey);

            //        var result = await ctx.AuthenticateAsync(downstreamRoute.AuthenticationOptions.AuthenticationProviderKey);

            //        if (result.Failure != null)
            //        {
            //            logger.LogInformation("route authenticated result " + result.Failure.Message);
            //        }
            //        logger.LogInformation("route authenticated result " + result.Principal.Identity.Name);

            //        ctx.User = result.Principal;

            //        if (!string.IsNullOrEmpty(token))
            //        {
            //            //if (!string.IsNullOrEmpty(token))
            //            //{
            //            //    ctx.Items.SetError(new UnauthenticatedError("your custom message"));

            //            //    return;
            //            //}
            //        }

            //        await next.Invoke(ctx);
            //    },
            //    AuthorizationMiddleware = async (ctx, next) =>
            //    {
            //        string token = ctx.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            //        var downstreamRoute = ctx.Items.DownstreamRoute();

            //        logger.LogInformation("route is authenticated scopes must be checked" + JsonSerializer.Serialize(ctx.User));

            //        if (!string.IsNullOrEmpty(token))
            //        {
            //            if (!string.IsNullOrEmpty(token))
            //            {
            //                ctx.Items.SetError(new UnauthenticatedError("your custom message" + JsonSerializer.Serialize(downstreamRoute.AuthenticationOptions.AllowedScopes)));

            //                return;
            //            }
            //        }

            //        await next.Invoke();
            //    }
            //};

            app.UseSwaggerForOcelotUI(opt =>
            {
                opt.PathToSwaggerGenerator = "/swagger/docs";
            }).UseOcelot(configuration).Wait();
        }
    }
}
