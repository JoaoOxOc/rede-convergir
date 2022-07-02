using MailChimp.Net;
using MailChimp.Net.Core;
using MailChimp.Net.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using UserService.Enum;
using UserService.Services.Database;

namespace UserService.Services.Mailchimp
{
    public class MailchimpRepository : IMailchimpRepository
    {
        private string ApiKey = "(your API key)";
        private MailChimpManager _mailChimpManager;
        private readonly IUnitOfWork _uow;

        public MailchimpRepository(IUnitOfWork uow)
        {
            _uow = uow;
            getMailchimpSettings();
            _mailChimpManager = new MailChimpManager(ApiKey);
        }

        private void getMailchimpSettings()
        {
            Expression<Func<CommonLibrary.Entities.Setting, bool>> filter = (x => x.Key == SettingCode.MailchimpApiKey.ToString());
            var settings = _uow.SettingRepository.Get(null, null, filter, "Key", CommonLibrary.Enums.SortDirection.Ascending);
            foreach (var setting in settings)
            {
                if (setting.Key == SettingCode.MailchimpApiKey.ToString())
                {
                    ApiKey = !string.IsNullOrEmpty(setting.Value) ? setting.Value : setting.DefaultValue;
                }
            }
        }

        /// <summary>
        /// creates a new campaign in mailchimp
        /// </summary>
        /// <param name="html">contains the content of your email using html notation</param>
        /// <param name="templateId">the mail template id to be used for the campaign</param>
        /// <param name="listId">the mailing list id to send the campaign</param>
        /// <param name="replyToEmail"></param>
        /// <param name="fromName"></param>
        /// <param name="emailTitle"></param>
        /// <param name="emailSubject"></param>
        /// <returns>the mail template content (model from mailchimp library) of the campaign</returns>
        public Content CreateAndSendCampaign(string html, int templateId, string listId, string replyToEmail, string fromName, string emailTitle, string emailSubject)
        {
            Setting _campaignSettings = new Setting
            {
                ReplyTo = replyToEmail,
                FromName = fromName,
                Title = emailTitle,
                SubjectLine = emailSubject,
            };
            var campaign = _mailChimpManager.Campaigns.AddAsync(new Campaign
            {
                Settings = _campaignSettings,
                Recipients = new Recipient { ListId = listId },
                Type = CampaignType.Regular
            }).Result;
            var timeStr = DateTime.Now.ToString();
            var content = _mailChimpManager.Content.AddOrUpdateAsync(
                campaign.Id,
                new ContentRequest()
                {
                    Template = new ContentTemplate
                    {
                        Id = templateId,
                        Sections = new Dictionary<string, object>()
                        {
                            { "body_content", html },
                            { "preheader_leftcol_content", $"<p>{timeStr}</p>" }
                        }
                    }
                }
            ).Result;
            _mailChimpManager.Campaigns.SendAsync(campaign.Id).Wait();
            return content;
        }

        /// <summary>
        /// Get all tags of a mailing list on mailchimp
        /// </summary>
        /// <param name="listId">the ID of the mailing list from where to get the tags of</param>
        /// <returns>The list of tags (model from mailchimp library)</returns>
        public List<ListTag> GetAllListTags(string listId)
          => _mailChimpManager.Tags.GetAllAsync(listId).Result.ToList();

        /// <summary>
        /// Get all the mail templates registered on mailchimp
        /// </summary>
        /// <returns>The list (model from mailchimp library) of mail templates</returns>
        public List<Template> GetAllTemplates()
          => _mailChimpManager.Templates.GetAllAsync().Result.ToList();

        /// <summary>
        /// Get all members registered on mailchimp
        /// </summary>
        /// <param name="listId">the ID of the mailing list from where to get the members of</param>
        /// <param name="listId">the subscription status of the members</param>
        /// <param name="offset">the offset of members to paginate the results</param>
        /// <param name="limit">the quantity of member records per pagination page</param>
        /// <param name="sortOrder">the sort order as "ASC" or "DESC"</param>
        /// <returns>The list of members (model from mailchimp library)</returns>
        public List<Member> GetAllMembers(string listId, Status? subscriptionStatus, int offset, int limit, MemberSortOrder sortOrder)
        {
            List<Member> members = _mailChimpManager.Members.GetAllAsync(listId, new MemberRequest { Offset = offset*limit, Limit = limit, SortOrder = sortOrder, Status= subscriptionStatus }).Result.ToList();
            return members;
        }

        /// <summary>
        /// Get all members count registered on mailchimp
        /// </summary>
        /// <param name="listId">the ID of the mailing list from where to get the members of</param>
        /// <param name="subscriptionStatus">the subscription status of the members</param>
        /// <returns>The quantity of members</returns>
        public int GetAllMembersCount(string listId, Status? subscriptionStatus)
        {
            return _mailChimpManager.Members.GetTotalItems(listId, subscriptionStatus).Result;
        }

        /// <summary>
        /// Get all clients registered on mailchimp
        /// </summary>
        /// <param name="listId">the ID of the mailing list from where to get the clients of</param>
        /// <returns>The list of clients (model from mailchimp library)</returns>
        public List<Client> GetAllClients(string listId)
          => _mailChimpManager.Clients.GetAllAsync(listId).Result.ToList();

        /// <summary>
        /// Get all the subscribers mailing lists registered on mailchimp
        /// </summary>
        /// <returns>The list (model from mailchimp library) of mailing lists</returns>
        public List<List> GetAllMailingLists()
          => _mailChimpManager.Lists.GetAllAsync().Result.ToList();

        /// <summary>
        /// Get mail template content by id
        /// </summary>
        /// <param name="templateId"></param>
        /// <returns>the mail template content (model from mailchimp library)</returns>
        public Content GetTemplateDefaultContent(string templateId)
          => (Content)_mailChimpManager.Templates.GetDefaultContentAsync(templateId).Result;

        /// <summary>
        /// Regists a new member with the specified email and tag (list)
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="tagName"></param>
        /// <param name="email"></param>
        /// <returns>the registered member data</returns>
        public async Task<Member> RegistNewMember(string listId, string tagName, string email)
        {
            var registered = await _mailChimpManager.Members.AddOrUpdateAsync(listId, new Member { EmailAddress = email, StatusIfNew = Status.Subscribed });
            if (registered != null && !string.IsNullOrEmpty(registered.Id) && !string.IsNullOrEmpty(tagName))
            {
                List<Tag> tags = new List<Tag>();
                tags.Add(new Tag { Name = tagName });
                await _mailChimpManager.Members.AddTagsAsync(listId, email, new Tags { MemberTags = tags });
            }
            return registered;
        }
    }
}
