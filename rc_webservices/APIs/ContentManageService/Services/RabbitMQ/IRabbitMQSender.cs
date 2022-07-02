using CommonLibrary.Entities.ViewModel;

namespace ContentManageService.Services.RabbitMQ
{
    public interface IRabbitMQSender
    {
        Task<bool> PublishEmailMessage(EmailVM emailData);
        Task<bool> PublishExceptionMessage(ExceptionModel exceptionData);
    }
}
