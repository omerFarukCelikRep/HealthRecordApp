using DataAccess.Abstract.IRepository;
using System.Threading.Tasks;

namespace DataAccess.Abstract.IConfiguration
{
    public interface IUnitOfWork
    {
        IUsersRepository Users { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IHealthDataRepository HealthDatas { get; }
        Task CompleteAsync();
    }
}
