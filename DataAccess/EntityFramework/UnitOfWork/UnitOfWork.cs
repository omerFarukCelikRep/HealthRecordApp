using DataAccess.Abstract.IConfiguration;
using DataAccess.Abstract.IRepository;
using DataAccess.EntityFramework.Context;
using DataAccess.EntityFramework.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DataAccess.EntityFramework.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AppDbContext _context;
        private readonly ILogger _logger;

        public IUsersRepository Users { get; private set; }
        public IRefreshTokenRepository RefreshTokens { get; private set; }
        public IHealthDataRepository HealthDatas { get; private set; }

        public UnitOfWork(AppDbContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger("db_logs");

            Users = new UsersRepository(context, _logger);
            RefreshTokens = new RefreshTokenRepository(context, _logger);
            HealthDatas = new HealthDataRepository(context, _logger);
        }

        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
