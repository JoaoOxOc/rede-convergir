using CommonLibrary.Entities.ViewModel;
using CommonLibrary.Enums;
using EmailService.Code;
using EmailService.Enums;
using EmailService.Model;
using EmailService.Repositories;
using EmailService.Sender;
using MassTransit;

namespace EmailService.Consumers
{
    public class ExceptionQueueConsumer : IExceptionQueueConsumer, IConsumer<CommonLibrary.Entities.ViewModel.ExceptionModel>
    {
        private readonly IEmailService _emailService;
        private readonly IEmailTemplatesRepository _emailTemplateRepo;
        private readonly ISettingsRepository _settingsRepository;

        private Setting? adminEmailSetting;

        public ExceptionQueueConsumer(IEmailService emailService, IEmailTemplatesRepository emailTemplateRepo, ISettingsRepository settingsRepository)
        {
            _emailService = emailService;
            _emailTemplateRepo = emailTemplateRepo;
            _settingsRepository = settingsRepository;

            loadNecessarySettings();
        }

        private void loadNecessarySettings()
        {
            Setting? adminEmail = this._settingsRepository.GetFiltered(SettingCode.Administration_Email.ToString(), null, null, "Key", "asc").FirstOrDefault();
            if (adminEmail != null)
            {
                adminEmailSetting = adminEmail;
            }
        }

        private List<Tuple<string,string>> loadExceptionBodyParams(ExceptionModel exData)
        {
            List<Tuple<string, string>> tokensToReplace_Body = new List<Tuple<string, string>>();

            tokensToReplace_Body.Add(new Tuple<string, string>("{{exceptionMessage}}", !string.IsNullOrEmpty(exData.Message) ? exData.Message : string.Empty));
            tokensToReplace_Body.Add(new Tuple<string, string>("{{exceptionStackTrace}}", !string.IsNullOrEmpty(exData.StackTrace) ? exData.StackTrace : string.Empty));
            if (exData.InnerException != null)
            {
                tokensToReplace_Body.Add(new Tuple<string, string>("{{innerExceptionMessage}}", exData.InnerException.Message));
                tokensToReplace_Body.Add(new Tuple<string, string>("{{innerExceptionStackTrace}}", !string.IsNullOrEmpty(exData.InnerException.StackTrace) ? exData.InnerException.StackTrace : string.Empty));
            }
            else
            {
                tokensToReplace_Body.Add(new Tuple<string, string>("{{innerExceptionMessage}}", string.Empty));
                tokensToReplace_Body.Add(new Tuple<string, string>("{{innerExceptionStackTrace}}", string.Empty));
            }
            tokensToReplace_Body.Add(new Tuple<string, string>("{{userId}}", !string.IsNullOrEmpty(exData.UserId) ? exData.UserId : string.Empty));
            
            tokensToReplace_Body.Add(new Tuple<string, string>("{{exceptionHappenedAt}}", exData.DateLogging != null && exData.DateLogging.HasValue ? exData.DateLogging.ToString() : DateTime.Now.ToString()));
            tokensToReplace_Body.Add(new Tuple<string, string>("{{methodInputData}}", !string.IsNullOrEmpty(exData.InputDataJson) ? exData.InputDataJson : string.Empty));

            return tokensToReplace_Body;
        }

        public async Task Consume(ConsumeContext<ExceptionModel> context)
        {
            if (adminEmailSetting != null)
            {
                EmailVM emailVM = new EmailVM();
                emailVM.To = new List<string>() { adminEmailSetting.Value };
                emailVM.EmailTemplateKey = EmailTemplatesEnum.AdminErrorNotification.ToString();
                emailVM.EmailLanguage = "en-US";
                emailVM.TokensToReplace_Body = loadExceptionBodyParams(context.Message);
                emailVM.TokensToReplace_Subject = new List<Tuple<string, string>>();

                var mailData = EmailTemplateReplacer.ProcessEmailTemplate(_emailTemplateRepo, emailVM);
                bool sent = _emailService.SendMail(mailData.To, mailData.Subject, mailData.Body);
                // TODO: deal with message not sent
            }
        }
    }
}
