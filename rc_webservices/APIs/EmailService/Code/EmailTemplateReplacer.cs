

using CommonLibrary.Entities.ViewModel;
using EmailService.Model;
using EmailService.Repositories;

namespace EmailService.Code
{
    public static class EmailTemplateReplacer
    {
        public static string TokenReplace_Body(string body, List<Tuple<string, string>> tokensToReplace_Body)
        {
            if (tokensToReplace_Body != null)
            {
                foreach (Tuple<string, string> token in tokensToReplace_Body)
                {
                    body = body.Replace(token.Item1, token.Item2);
                }
            }

            return body;
        }

        public static string TokenReplace_Subject(string subject, List<Tuple<string, string>> tokensToReplace_Subject)
        {
            if (tokensToReplace_Subject != null)
            {
                foreach (Tuple<string, string> token in tokensToReplace_Subject)
                {
                    subject = subject.Replace(token.Item1, token.Item2);
                }
            }

            return subject;
        }

        public static EmailVM ProcessEmailTemplate(IEmailTemplatesRepository templatesRepository, EmailVM emailData)
        {
            emailData.EmailLanguage = !string.IsNullOrEmpty(emailData.EmailLanguage) ? emailData.EmailLanguage : "en-US";
            EmailTemplate template = templatesRepository.GetFiltered(emailData.EmailTemplateKey, emailData.EmailLanguage, null, null, string.Empty, "asc").ToList<EmailTemplate>().FirstOrDefault();
            if (template != null)
            {
                emailData.Subject = TokenReplace_Subject(template.Subject, emailData.TokensToReplace_Subject);

                emailData.Body = TokenReplace_Subject(template.BodyHtml, emailData.TokensToReplace_Body);
            }
            return emailData;
        }

    }
}
