


using EmailService;

namespace EmailService.Repositories
{
    public interface ISettingsRepository
    {
        Task<IEnumerable<Model.Setting>> Get();

        IEnumerable<Model.Setting> GetFiltered(string searchText, int? offset, int? limit, string sort, string sortDirection, string ignoreId = null);

        int Count(string searchText, int? offset, int? limit, string sort);

        Task<Model.Setting> GetById(object id);

        Task Add(Model.Setting entity);

        Task<Model.Setting> UpdateSetting(Model.Setting editedSetting);

        Task<bool> Delete(string id);
    }
}
