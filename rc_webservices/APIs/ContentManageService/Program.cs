using ContentManageService.Services.Database;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using ContentManageService.Migrations.Config;
using MicroservicesLibrary.HttpHandlers;
using ContentManageService.Services;
using ContentManageService.Services.RabbitMQ;
using ContentManageService.Binders;
using ContentManageService.Services.HierarchyManager;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var connStringBuilder = new NpgsqlConnectionStringBuilder();
int dbPort = 0;
int.TryParse(builder.Configuration["DatabaseSettings:DbPort"], out dbPort);
bool dbPooling = false;
bool.TryParse(builder.Configuration["DatabaseSettings:DbPooling"], out dbPooling);
bool dbTrustCrt = false;
bool.TryParse(builder.Configuration["DatabaseSettings:DbTrustCertificate"], out dbTrustCrt);
connStringBuilder.Host = builder.Configuration["DatabaseSettings:DbHost"];
connStringBuilder.Port = dbPort;
connStringBuilder.SslMode = builder.Configuration["DatabaseSettings:DbSslMode"] == "None" ? SslMode.Disable : SslMode.Require;
connStringBuilder.Username = builder.Configuration["DatabaseSettings:DbUser"];
connStringBuilder.Password = builder.Configuration["DatabaseSettings:DbPassword"];
connStringBuilder.Database = builder.Configuration["DatabaseSettings:Database"];
connStringBuilder.TrustServerCertificate = dbTrustCrt;
connStringBuilder.Pooling = dbPooling;
connStringBuilder.ServerCompatibilityMode = builder.Configuration["DatabaseSettings:DbServerCompatibilityMode"] == "Redshift" ? ServerCompatibilityMode.Redshift : ServerCompatibilityMode.None;
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");
builder.Services.AddDbContext<DatabaseContext>(x => x.UseNpgsql(connStringBuilder.ConnectionString));

// Add services to the container.
builder.Services.TryAddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.TryAddSingleton<IRabbitMQSender, RabbitMQSender>();

builder.Services.ConfigureMassTransitRabbitMQ(configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwagger();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    try
    {
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
    }
    DbInitializer.Initialize(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ContentManagementService v1"));

app.UseAuthorization();

// global error handler
// read: https://jasonwatmore.com/post/2022/01/24/net-6-jwt-authentication-with-refresh-tokens-tutorial-with-example-api
app.UseMiddleware<ErrorHandlerMiddleware>();

app.MapControllers();

app.Run();
