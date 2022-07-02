using CommonLibrary.Enums;
using System.Linq.Expressions;
using UserService.Entities;
using UserService.Services.Database;

namespace UserService.Services.TermsManager
{
    public class TermsManager : ITermsManager
    {
        private readonly IUnitOfWork _uow;

        public TermsManager(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public decimal GetNewTermsConditionsVersion()
        {
            decimal newVersion = 0;
            var termsLastVersion = _uow.TermsConditionsRepository.Get(1, 1, null, "Version", SortDirection.Descending).FirstOrDefault();
            if (termsLastVersion != null)
            {
                newVersion = termsLastVersion.Version + 1;
            }
            else
            {
                newVersion = 1;
            }

            return newVersion;
        }

        public decimal GetActiveTermsConditionsVersion()
        {
            decimal currentVersion = 0;
            var termsLastVersion = _uow.TermsConditionsRepository.Get(1, 1, null, "Version", SortDirection.Descending).FirstOrDefault();
            if (termsLastVersion != null)
            {
                currentVersion = termsLastVersion.Version;
            }
            else
            {
                currentVersion = 1;
            }

            return currentVersion;
        }

        public bool DeactivateTermsConditions(TermsConditions termsData)
        {
            bool deactivated = false;
            Expression<Func<TermsConditions, bool>> filter = (x => x.IsActive == true && x.Id != termsData.Id);
            var termsActiveFound = this._uow.TermsConditionsRepository.Get(null, null, filter, "Version", SortDirection.Ascending);
            if (termsActiveFound != null && termsActiveFound.Count > 0)
            {
                foreach (var termsCondition in termsActiveFound)
                {
                    termsCondition.IsActive = false;
                }
                _uow.TermsConditionsRepository.Update(termsActiveFound);
                _uow.Save();
                deactivated = true;
            }
            else
            {
                deactivated = true;
            }
            return deactivated;
        }
    }
}
