using CommonLibrary.Entities.ViewModel;
using MassTransit;
using MicroservicesLibrary.Exceptions;

namespace ContentManageService.Services.RabbitMQ
{
    public class RabbitMQSender : IRabbitMQSender
    {
        private readonly IConfiguration _configuration;
        private readonly IBus _bus;
        private readonly string rabbitMqHost;
        private readonly string[] rabbitMqExchanges;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration">dependency injection of app configuration container</param>
        /// <param name="bus">dependency injection of MassTransit bus object</param>
        public RabbitMQSender(IConfiguration configuration, IBus bus)
        {
            _configuration = configuration;
            _bus = bus;
            rabbitMqHost = _configuration.GetSection("ApplicationSettings:RabbitMQHost").Get<string>();
            string rabbitMqExchangesString = _configuration.GetSection("ApplicationSettings:RabbitMQExchanges").Get<string>();
            rabbitMqExchanges = !string.IsNullOrEmpty(rabbitMqExchangesString) ? rabbitMqExchangesString.Split(',') : System.Array.Empty<string>();
        }

        public async Task<bool> PublishEmailMessage(EmailVM emailData)
        {
            bool success = false;
            try
            {

                Uri uri = new Uri("rabbitmq://" + rabbitMqHost + "/" + Array.Find(rabbitMqExchanges, x => x.Contains("email")));
                var endPoint = await _bus.GetSendEndpoint(uri);
                await endPoint.Send(emailData);
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
                throw new AppException(ex.Message);
            }
            return success;
        }

        public async Task<bool> PublishExceptionMessage(ExceptionModel exceptionData)
        {
            bool success = false;
            try
            {

                Uri uri = new Uri("rabbitmq://" + rabbitMqHost + "/" + Array.Find(rabbitMqExchanges, x => x.Contains("exception")));
                var endPoint = await _bus.GetSendEndpoint(uri);
                await endPoint.Send(exceptionData);
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
                throw new AppException(ex.Message);
            }
            return success;
        }
    }
}
