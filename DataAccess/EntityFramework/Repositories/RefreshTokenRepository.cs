using DataAccess.Abstract.IRepository;
using DataAccess.EntityFramework.Context;
using Entity.Concrete;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.EntityFramework.Repositories
{
    public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext context, ILogger logger) : base(context, logger)
        {
        }

        public async Task<RefreshToken> GetByRefreshToken(string refreshToken)
        {
            try
            {
                return await _table.Where(x => x.Token.ToLower() == refreshToken.ToLower())
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} GetByRefreshToken method has generated an error", typeof(RefreshToken));
                return null;
            }
        }

        public async Task<bool> MarkRefreshTokenAsUsed(RefreshToken refreshToken)
        {
            try
            {
                var token = await _table.Where(x => x.Token.ToLower() == refreshToken.Token.ToLower())
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();

                if (token == null)
                {
                    return false;
                }

                refreshToken.IsUsed = true;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} MarkRefreshTokenAsUsed method has generated an error", typeof(RefreshToken));
                return false;
            }
        }
    }
}
