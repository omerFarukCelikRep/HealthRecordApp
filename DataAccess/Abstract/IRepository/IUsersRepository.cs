using Entity.Concrete;
using System.Threading.Tasks;

namespace DataAccess.Abstract.IRepository
{
    public interface IUsersRepository : IBaseRepository<AppUser>
    {
    }
}
