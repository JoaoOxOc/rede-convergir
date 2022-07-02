using CommonLibrary.Entities;
using Microsoft.AspNetCore.Identity;
using UserService.Entities;
using UserService.Repositories;

namespace UserService.Services.Database
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

        private IGenericRepository<User> _userRepository;
        private IGenericRepository<Association> _associationRepository;
        private IGenericRepository<AssociationType> _associationTypeRepository;
        private IGenericRepository<AssociationProfileTranslation> _associationProfileTranslationRepository;
        private IGenericRepository<TermsConditions> _termsConditionsRepository;
        private IGenericRepository<TermsConditionsTranslation> _termsConditionsTranslationRepository;
        private IGenericRepository<RoleScope> _roleScopeRepository;
        private IGenericRepository<Scope> _scopeRepository;
        private IGenericRepository<Setting> _settingRepository;
        private IGenericRepository<IdentityRole> _identityRoleRepository;
        private IGenericRepository<IdentityUserRole<string>> _identityUserRoleRepository;
        private IGenericRepository<ClientCredential> _clientCredentialRepository;
        private IGenericRepository<NewsletterSubscription> _newsletterSubscriptionRepository;

        private readonly DatabaseContext dbContext;

        public IGenericRepository<User> UserRepository
        {
            get
            {
                return _userRepository = _userRepository ?? new GenericRepository<User>(dbContext);
            }
        }

        public IGenericRepository<Setting> SettingRepository
        {
            get
            {
                return _settingRepository = _settingRepository ?? new GenericRepository<Setting>(dbContext);
            }
        }

        public IGenericRepository<Association> AssociationRepository
        {
            get
            {
                return _associationRepository = _associationRepository ?? new GenericRepository<Association>(dbContext);
            }
        }

        public IGenericRepository<AssociationType> AssociationTypeRepository
        {
            get
            {
                return _associationTypeRepository = _associationTypeRepository ?? new GenericRepository<AssociationType>(dbContext);
            }
        }

        public IGenericRepository<AssociationProfileTranslation> AssociationProfileTranslationRepository
        {
            get
            {
                return _associationProfileTranslationRepository = _associationProfileTranslationRepository ?? new GenericRepository<AssociationProfileTranslation>(dbContext);
            }
        }

        public IGenericRepository<TermsConditions> TermsConditionsRepository
        {
            get
            {
                return _termsConditionsRepository = _termsConditionsRepository ?? new GenericRepository<TermsConditions>(dbContext);
            }
        }


        public IGenericRepository<TermsConditionsTranslation> TermsConditionsTranslationRepository
        {
            get
            {
                return _termsConditionsTranslationRepository = _termsConditionsTranslationRepository ?? new GenericRepository<TermsConditionsTranslation>(dbContext);
            }
        }


        public IGenericRepository<ClientCredential> ClientCredentialRepository
        {
            get
            {
                return _clientCredentialRepository = _clientCredentialRepository ?? new GenericRepository<ClientCredential>(dbContext);
            }
        }

        public IGenericRepository<RoleScope> RoleScopeRepository
        {
            get
            {
                return _roleScopeRepository = _roleScopeRepository ?? new GenericRepository<RoleScope>(dbContext);
            }
        }

        public IGenericRepository<Scope> ScopeRepository
        {
            get
            {
                return _scopeRepository = _scopeRepository ?? new GenericRepository<Scope>(dbContext);
            }
        }

        public IGenericRepository<IdentityRole> IdentityRoleRepository
        {
            get
            {
                return _identityRoleRepository = _identityRoleRepository ?? new GenericRepository<IdentityRole>(dbContext);
            }
        }

        public IGenericRepository<IdentityUserRole<string>> IdentityUserRoleRepository
        {
            get
            {
                return _identityUserRoleRepository = _identityUserRoleRepository ?? new GenericRepository<IdentityUserRole<string>>(dbContext);
            }
        }

        public IGenericRepository<NewsletterSubscription> NewsletterSubscriptionRepository
        {
            get
            {
                return _newsletterSubscriptionRepository = _newsletterSubscriptionRepository ?? new GenericRepository<NewsletterSubscription>(dbContext);
            }
        }
    }
}
