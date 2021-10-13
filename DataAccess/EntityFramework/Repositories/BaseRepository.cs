using DataAccess.Abstract.IRepository;
using DataAccess.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DataAccess.EntityFramework.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected AppDbContext _context;
        internal DbSet<T> _table;
        protected readonly ILogger _logger;

        public BaseRepository(AppDbContext context, ILogger logger)
        {
            _context = context;
            _table = _context.Set<T>();
            _logger = logger;
        }

        public virtual async Task<bool> Add(T entity)
        {
            await _table.AddAsync(entity);

            return true;
        }

        public virtual Task<bool> Delete(T entity, string userId)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<T> Get(Expression<Func<T, bool>> expression)
        {
            return await _table.FirstAsync(expression);
        }

        public virtual async Task<IEnumerable<T>> GetAll()
        {
            return await _table.ToListAsync();
        }

        public virtual async Task<T> GetById(Guid id)
        {
            return await _table.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>> expression)
        {
            return await _table.Where(expression).ToListAsync();
        }

        public virtual Task<bool> Update(T entity)
        {
            throw new NotImplementedException();
        }
    }
}
