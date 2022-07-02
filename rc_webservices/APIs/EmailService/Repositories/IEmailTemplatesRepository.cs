using EmailService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Repositories
{
    public interface IEmailTemplatesRepository
    {
        Task<IEnumerable<EmailTemplate>> Get();

        IEnumerable<EmailTemplate> GetFiltered(string searchText, string language, int? offset, int? limit, string sort, string sortDirection, string ignoreId = null);

        int Count(string searchText, string language, int? offset, int? limit, string sort);

        Task<EmailTemplate> GetById(object id);

        Task Add(EmailTemplate entity);

        Task<EmailTemplate> Update(EmailTemplate editedEmailTemplate);

        Task<bool> Delete(string id);
    }
}
