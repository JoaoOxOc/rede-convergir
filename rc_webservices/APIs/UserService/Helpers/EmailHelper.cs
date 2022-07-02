using CommonLibrary.Entities.ViewModel;
using System.Security.Claims;
using UserService.Entities;
using UserService.Services.RabbitMQ;
using MicroservicesLibrary.Exceptions;

namespace UserService.Helpers
{
    public class EmailHelper
    {
        private readonly IRabbitMQSender _rabbitMqSender;


        public EmailHelper(IRabbitMQSender rabbitMqSender)
        {
            _rabbitMqSender = rabbitMqSender;
        }


        public async Task<bool> SendEmailToUser(User user, string emailTemplateKey)
        {
            EmailVM emailData = new EmailVM();
            emailData.To = new List<string> { user.Email };

            emailData.Body = "Please access the following url to reset your password, you only have 15 minutes to reset your password:<br/>";
            emailData.Subject = "Reset your password";
            emailData.EmailTemplateKey = emailTemplateKey;

            bool success = await _rabbitMqSender.PublishEmailMessage(emailData);
            if (!success)
            {
                throw new AppException("Email send error");
            }
            return success;
        }

        public async Task<bool> SendEmailToUserWithLink(User user, string link, string emailTemplateKey)
        {
            EmailVM emailData = new EmailVM();
            emailData.To = new List<string> { user.Email };

            emailData.Body = "Please access the following url to reset your password, you only have 15 minutes to reset your password:<br/><a target='_blank' rel='noopener noreferrer' href='" + link + "'>" + link + "</a>";
            emailData.Subject = "Reset your password";
            emailData.EmailTemplateKey = emailTemplateKey;

            bool success = await _rabbitMqSender.PublishEmailMessage(emailData);
            if (!success)
            {
                throw new AppException("Email send error");
            }
            return success;
        }
    }
}
