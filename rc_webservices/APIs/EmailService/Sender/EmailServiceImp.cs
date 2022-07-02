
using EmailService.Code;
using EmailService.Enums;
using EmailService.Repositories;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EmailService.Sender
{
    public class EmailServiceImp : IEmailService
    {
        private readonly ISettingsRepository _settingsRepository;

        public EmailServiceImp(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
            loadEmailSettings();
        }

        private void loadEmailSettings()
        {
            var allSettings = _settingsRepository.Get().Result;

            SMTPConfigurations.Server = allSettings.SingleOrDefault(x => x.Key == SettingCode.SMTP_Server.ToString()).Value;
            SMTPConfigurations.Username = allSettings.SingleOrDefault(x => x.Key == SettingCode.SMTP_Username.ToString()).Value;
            SMTPConfigurations.Password = allSettings.SingleOrDefault(x => x.Key == SettingCode.SMTP_Password.ToString()).Value;
            SMTPConfigurations.Port = Convert.ToInt32(allSettings.SingleOrDefault(x => x.Key == SettingCode.SMTP_Port.ToString()).Value);
            SMTPConfigurations.From = allSettings.SingleOrDefault(x => x.Key == SettingCode.SMTP_From.ToString()).Value;
            SMTPConfigurations.EnableSSL = Convert.ToBoolean(allSettings.SingleOrDefault(x => x.Key == SettingCode.SMTP_SSL.ToString()).Value);
            SMTPConfigurations.FromName = allSettings.SingleOrDefault(x => x.Key == SettingCode.SMTP_FromName.ToString()).Value;
        }

        public bool SendMail(string to, string subject, string body)
        {
            List<string> tos = new List<string>();
            tos.Add(to);

            List<string> attachments = new List<string>();

            return SendMail(tos, attachments, subject, body);
        }

        public bool SendMail(List<string> to, string subject, string body)
        {
            List<string> attachments = new List<string>();

            return SendMail(to, attachments, subject, body);
        }

        public bool SendMail(string to, string attachment, string subject, string body)
        {
            List<string> tos = new List<string>();
            tos.Add(to);

            List<string> attachments = new List<string>();
            attachments.Add(attachment);

            return SendMail(tos, attachments, subject, body);
        }

        public bool SendMail(string to, List<string> attachments, string subject, string body)
        {
            List<string> tos = new List<string>();
            tos.Add(to);

            return SendMail(tos, attachments, subject, body);
        }

        public bool SendMail(List<string> to, string attachment, string subject, string body)
        {
            List<string> attachments = new List<string>();
            attachments.Add(attachment);

            return SendMail(to, attachments, subject, body);
        }

        private static bool SendMail(List<string> to, List<string> attachments, string subject, string body)
        {
            try
            {
                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress(SMTPConfigurations.From, SMTPConfigurations.FromName, Encoding.UTF8);
                    mail.IsBodyHtml = true;
                    
                    foreach (string s in to)
                    {
                        mail.To.Add(s);
                    }

                    SmtpClient client = new SmtpClient();
                
                    client.Port = SMTPConfigurations.Port;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Host = SMTPConfigurations.Server;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(SMTPConfigurations.Username, SMTPConfigurations.Password);
                    client.EnableSsl = SMTPConfigurations.EnableSSL; 
                    if (client.EnableSsl)
                    {
                        ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                    }
                   
                    mail.Subject = subject;
                    mail.Body = body;

                    mail.SubjectEncoding = Encoding.GetEncoding("iso-8859-1");
                    mail.BodyEncoding = Encoding.GetEncoding("iso-8859-1");

                    SetAttachments(attachments, ref mail);

                    client.Send(mail);

                    return true;
            }
            catch (SmtpException ex)
            {
                string extraInfo = string.Empty;
                if (to != null && to.Count > 0)
                {
                    extraInfo = string.Join(", ", to);
                }

                var errorLog = ErrorLog.ErrorLogGenerator(ex, null, "EmailService", 0, string.Empty, string.Empty, extraInfo);

                //rabbitMQ.ExchangeChannel.BasicPublish("gdpr", "log.event.create", true, null, bytes);
                throw new Exception(errorLog);
                if (ex.StatusCode == SmtpStatusCode.MailboxNameNotAllowed || ex.StatusCode == SmtpStatusCode.MailboxUnavailable)
                {
                    // If is an incorrect email delete the event fron the queue
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                string extraInfo = string.Empty;
                if (to != null && to.Count > 0)
                {
                    extraInfo = string.Join(", ", to);
                }

                var errorLog = ErrorLog.ErrorLogGenerator(ex, null, "EmailService", 0, string.Empty, string.Empty, extraInfo);
                throw new Exception(errorLog);
                //rabbitMQ.ExchangeChannel.BasicPublish("gdpr", "log.event.create", true, null, bytes);

                return false;
            }
        }
        
        private static void SetAttachments(List<string> attachments, ref MailMessage mail)
        {
            foreach (string file in attachments)
            {
                Attachment tmp = new Attachment(file);
                mail.Attachments.Add(tmp);
            }
        }
    }
}
