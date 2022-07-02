using CommonLibrary.Entities.ViewModel;
using MassTransit;
using System.Threading.Tasks;

namespace EmailService.Consumers
{
    public interface IEmailQueueConsumer
    {
        Task Consume(ConsumeContext<EmailVM> context);
    }
}
