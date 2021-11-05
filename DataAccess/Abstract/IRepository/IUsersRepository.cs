using Entity.Concrete;
using System;
using System.Threading.Tasks;

namespace DataAccess.Abstract.IRepository
{
    public interface IUsersRepository : IBaseRepository<AppUser>
    {
        Task<bool> UpdateUserProfile(AppUser user);
        Task<AppUser> GetByIdentityId(Guid identityId);
    }
}
