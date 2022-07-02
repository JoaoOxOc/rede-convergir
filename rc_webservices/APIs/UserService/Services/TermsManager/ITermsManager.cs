using UserService.Entities;

namespace UserService.Services.TermsManager
{
    public interface ITermsManager
    {
        decimal GetNewTermsConditionsVersion();

        decimal GetActiveTermsConditionsVersion();

        bool DeactivateTermsConditions(TermsConditions termsData);
    }
}
