using CommonLibrary.Entities;
using EmailService.Consumers;
using EmailService.Repositories;
using EmailService.Sender;
using EmailService.Services;
using EmailService.Services.RabbitMQ;
using MassTransit;
using MicroservicesLibrary.HttpHandlers;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

builder.Services.Configure<DbConfiguration>(options =>
{
    options.ConnectionString
        = configuration.GetSection("ConnectionStrings:DefaultConnectionString").Value;
    options.Database
        = configuration.GetSection("ConnectionStrings:Database").Value;
});

// Add services to the container.
builder.Services.TryAddScoped<ISettingsRepository, SettingsRepository>();
builder.Services.TryAddScoped<IEmailTemplatesRepository, EmailTemplatesRepository>();
builder.Services.TryAddScoped<IEmailService, EmailServiceImp>();

builder.Services.AddHttpClient<IEmailQueueConsumer, EmailQueueConsumer>();
builder.Services.AddHttpClient<IExceptionQueueConsumer, ExceptionQueueConsumer>();
builder.Services.TryAddSingleton<IRabbitMQSender, RabbitMQSender>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwagger();

builder.Services.ConfigureRabbitConsumer(configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

using (var scope = app.Services.CreateScope())
{
    DbInitializer.Initialize(scope.ServiceProvider);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

// global error handler
// read: https://jasonwatmore.com/post/2022/01/24/net-6-jwt-authentication-with-refresh-tokens-tutorial-with-example-api
app.UseMiddleware<ErrorHandlerMiddleware>();

app.MapControllers();

app.Run();
