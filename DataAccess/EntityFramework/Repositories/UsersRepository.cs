using DataAccess.Abstract.IRepository;
using DataAccess.EntityFramework.Context;
using Entity.Concrete;
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
        //private readonly AppDbContext _context;

        public UsersRepository(AppDbContext context, ILogger logger) : base(context, logger)
        {
            //_context = context;
        }

        public override async Task<IEnumerable<AppUser>> GetAll()
        {
            try
            {
                return await _table.Where(a => a.Status != Entity.Enums.Status.Passive)
                                    .AsNoTracking()
                                    .ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} All method has generated an error", typeof(UsersRepository));

                return new List<AppUser>();
            }

        }
    }
}
