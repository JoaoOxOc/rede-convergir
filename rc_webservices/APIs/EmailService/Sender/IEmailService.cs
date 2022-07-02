namespace EmailService.Sender
{
    public interface IEmailService
    {
        bool SendMail(string to, string subject, string body);
        bool SendMail(List<string> to, string subject, string body);
        bool SendMail(string to, string attachment, string subject, string body);
        bool SendMail(string to, List<string> attachments, string subject, string body);
        bool SendMail(List<string> to, string attachment, string subject, string body);
    }
}
