using System.Threading.Tasks;

namespace tpGateway.Services
{
    public interface ITokenManager
    {
        Task<bool> ValidateClient(string clientId, string clientToken);
    }
}
