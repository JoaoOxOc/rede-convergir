using CommonLibrary.Entities;
using ContentManageService.Entities;
using ContentManageService.Repositories;

namespace ContentManageService.Services.Database
{
    public interface IUnitOfWork
    {
        IGenericRepository<Setting> SettingRepository { get; }
        IGenericRepository<Banner> BannerRepository { get; }
        IGenericRepository<BannerTranslation> BannerTranslationRepository { get; }

        void Save();
    }
}
