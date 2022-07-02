using MassTransit;

namespace EmailService.Consumers
{
    public interface IExceptionQueueConsumer
    {
        Task Consume(ConsumeContext<CommonLibrary.Entities.ViewModel.ExceptionModel> context);
    }
}
