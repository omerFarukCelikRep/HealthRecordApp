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
    public class HealthDataRepository : BaseRepository<HealthData>, IHealthDataRepository
    {
        public HealthDataRepository(AppDbContext context, ILogger logger) : base(context,logger)
        {

        }

        public override async Task<IEnumerable<HealthData>> GetAll()
        {
            try
            {
                return await _table.Where(a => a.Status != Status.Passive)
                                    .AsNoTracking()
                                    .ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} GetAll method has generated an error", typeof(HealthDataRepository));

                return new List<HealthData>();
            }

        }

        public async Task<bool> UpdateHealthData(HealthData healthData)
        {
            try
            {
                var existingHealthData = await _table.Where(a => a.Status != Status.Passive && a.Id == healthData.Id).FirstOrDefaultAsync();

                if (existingHealthData == null)
                {
                    return false;
                }

                existingHealthData.BloodType = healthData.BloodType;
                existingHealthData.Height = healthData.Height;
                existingHealthData.Race = healthData.Race;
                existingHealthData.Weight = healthData.Weight;
                existingHealthData.UseGlasses = healthData.UseGlasses;
                existingHealthData.ModifiedDate = DateTime.UtcNow;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} UpdateHealthData method has generated an error", typeof(HealthDataRepository));

                return false;
            }
        }
    }
}
