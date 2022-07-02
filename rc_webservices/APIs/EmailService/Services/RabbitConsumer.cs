using EmailService.Consumers;
using GreenPipes;
using MassTransit;

namespace EmailService.Services
{
    public static class RabbitConsumer
    {
        /// <summary>
        /// Injects a rabbit MQ consumer configuration
        /// https://masstransit-project.com/advanced/connect-endpoint.html
        /// </summary>
        /// <param name="services">the dependency injection container</param>
        /// <returns></returns>
        public static IServiceCollection ConfigureRabbitConsumer(this IServiceCollection services, IConfiguration configuration)
        {
            string rabbitMqHost = configuration.GetSection("ApplicationSettings:RabbitMQHost").Get<string>();
            string rabbitMqUser = configuration.GetSection("ApplicationSettings:RabbitMQUser").Get<string>();
            string rabbitMqPassword = configuration.GetSection("ApplicationSettings:RabbitMQPassword").Get<string>();
            string rabbitMqEmailQueue = configuration.GetSection("ApplicationSettings:RabbitMQEmailQueue").Get<string>();
            string rabbitMqExceptionQueue = configuration.GetSection("ApplicationSettings:RabbitMQExceptionQueue").Get<string>();
            string rabbitMqExchangesString = configuration.GetSection("ApplicationSettings:RabbitMQExchanges").Get<string>();
            string[] rabbitMqExchanges = !string.IsNullOrEmpty(rabbitMqExchangesString) ? rabbitMqExchangesString.Split(',') : System.Array.Empty<string>();
            services.AddMassTransit(x =>
            {
                x.AddConsumer<EmailQueueConsumer>();
                x.AddConsumer<ExceptionQueueConsumer>();
                //x.UsingRabbitMq((context, cfg) =>
                //    cfg.ConfigureEndpoints(context));

                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(new Uri("rabbitmq://" + rabbitMqHost), h =>
                    {
                        h.Username(rabbitMqUser);
                        h.Password(rabbitMqPassword);
                    });
                    cfg.ReceiveEndpoint(rabbitMqEmailQueue, ep =>
                    {
                        ep.Bind(Array.Find(rabbitMqExchanges, x => x.Contains("email")));
                        ep.PrefetchCount = 16;
                        ep.UseMessageRetry(r => r.Interval(2, 100));
                        ep.ConfigureConsumer<EmailQueueConsumer>(provider);
                    });
                    cfg.ReceiveEndpoint(rabbitMqExceptionQueue, ep =>
                    {
                        ep.Bind(Array.Find(rabbitMqExchanges, x => x.Contains("exception")));
                        ep.PrefetchCount = 16;
                        ep.UseMessageRetry(r => r.Interval(2, 100));
                        ep.ConfigureConsumer<ExceptionQueueConsumer>(provider);
                    });
                }));
            });
            services.AddMassTransitHostedService();
            return services;
        }
    }
}
