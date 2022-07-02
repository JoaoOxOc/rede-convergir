using CommonLibrary.Entities.ViewModel;
using EmailService.Code;
using EmailService.Repositories;
using EmailService.Sender;
using MassTransit;

namespace EmailService.Consumers
{
    public class EmailQueueConsumer : IEmailQueueConsumer, IConsumer<EmailVM>
    {
        private readonly IEmailService _emailService;
        private readonly IEmailTemplatesRepository _emailTemplateRepo;

        public EmailQueueConsumer(IEmailService emailService, IEmailTemplatesRepository emailTemplateRepo)
        {
            _emailService = emailService;
            _emailTemplateRepo = emailTemplateRepo;
        }

        public async Task Consume(ConsumeContext<EmailVM> context)
        {
            var mailData = EmailTemplateReplacer.ProcessEmailTemplate(_emailTemplateRepo, context.Message);
            bool sent = _emailService.SendMail(mailData.To, mailData.Subject, mailData.Body);
            // TODO: deal with message not sent
        }
    }
}
