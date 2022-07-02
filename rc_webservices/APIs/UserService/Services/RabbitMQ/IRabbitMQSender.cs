using CommonLibrary.Entities.ViewModel;

namespace UserService.Services.RabbitMQ
{
    public interface IRabbitMQSender
    {
        Task<bool> PublishEmailMessage(EmailVM emailData);
        Task<bool> PublishExceptionMessage(ExceptionModel exceptionData);
    }
}
