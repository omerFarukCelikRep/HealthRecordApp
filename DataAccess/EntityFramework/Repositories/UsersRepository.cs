using DataAccess.Abstract.IRepository;
using DataAccess.EntityFramework.Context;
using Entity.Concrete;
using Entity.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.EntityFramework.Repositories
{
    public class UsersRepository : BaseRepository<AppUser>, IUsersRepository
    {
        public UsersRepository(AppDbContext context, ILogger logger) : base(context, logger)
        {
        }

        public override async Task<IEnumerable<AppUser>> GetAll()
        {
            try
            {
                return await _table.Where(a => a.Status != Status.Passive)
                                    .AsNoTracking()
                                    .ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} GetAll method has generated an error", typeof(UsersRepository));

                return new List<AppUser>();
            }

        }

        public async Task<AppUser> GetByIdentityId(Guid identityId)
        {
            try
            {
                return await _table.Where(a => a.Status != Status.Passive && a.IdentityId == identityId)
                                    .FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} GetByIdentityId method has generated an error", typeof(UsersRepository));

                return null;
            }
        }

        public async Task<bool> UpdateUserProfile(AppUser user)
        {
            try
            {
                var existingUser = await _table.Where(a => a.Status != Status.Passive && a.Id == user.Id).FirstOrDefaultAsync();

                if (existingUser == null)
                {
                    return false;
                }

                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.MobileNumber = user.MobileNumber;
                existingUser.Phone = user.Phone;
                existingUser.Address = user.Address;
                existingUser.Sex = user.Sex;
                existingUser.ModifiedDate = DateTime.UtcNow;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} UpdateUserProfile method has generated an error", typeof(UsersRepository));

                return false;
            }
        }


    }
}
