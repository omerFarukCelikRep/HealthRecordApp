using Entity.Concrete;
using System.Threading.Tasks;

namespace DataAccess.Abstract.IRepository
{
    public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
    {
        Task<RefreshToken> GetByRefreshToken(string refreshToken);
        Task<bool> MarkRefreshTokenAsUsed(RefreshToken refreshToken);
    }
}
