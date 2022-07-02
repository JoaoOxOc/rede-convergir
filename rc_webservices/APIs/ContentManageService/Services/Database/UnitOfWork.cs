using CommonLibrary.Entities;
using ContentManageService.Entities;
using ContentManageService.Repositories;

namespace ContentManageService.Services.Database
{
    public class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(DatabaseContext context)
        {
            dbContext = context;
        }

        public void Save()
        {
            dbContext.SaveChanges();
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }

        private IGenericRepository<Banner> _bannerRepository;
        private IGenericRepository<BannerTranslation> _bannerTranslationRepository;
        private IGenericRepository<Setting> _settingRepository;

        private readonly DatabaseContext dbContext;

        public IGenericRepository<Setting> SettingRepository
        {
            get
            {
                return _settingRepository = _settingRepository ?? new GenericRepository<Setting>(dbContext);
            }
        }

        public IGenericRepository<Banner> BannerRepository
        {
            get
            {
                return _bannerRepository = _bannerRepository ?? new GenericRepository<Banner>(dbContext);
            }
        }

        public IGenericRepository<BannerTranslation> BannerTranslationRepository
        {
            get
            {
                return _bannerTranslationRepository = _bannerTranslationRepository ?? new GenericRepository<BannerTranslation>(dbContext);
            }
        }

        
    }
}
