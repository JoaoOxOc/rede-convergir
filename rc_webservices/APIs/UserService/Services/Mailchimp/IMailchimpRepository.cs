using MailChimp.Net.Core;
using MailChimp.Net.Models;

namespace UserService.Services.Mailchimp
{
    public interface IMailchimpRepository
    {
        List<Template> GetAllTemplates();
        List<Member> GetAllMembers(string listId, Status? subscriptionStatus, int offset, int limit, MemberSortOrder sortOrder);
        int GetAllMembersCount(string listId, Status? subscriptionStatus);
        Task<Member> RegistNewMember(string listId, string tagName, string email);
        List<Client> GetAllClients(string listId);
        List<List> GetAllMailingLists();
        Content GetTemplateDefaultContent(string templateId);
        Content CreateAndSendCampaign(string html, int templateId, string listId, string replyToEmail, string fromName, string emailTitle, string emailSubject);
    }
}
