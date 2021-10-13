using Entity.Concrete;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.EntityFramework.Context
{
    public class AppDbContext : IdentityDbContext
    {
        public virtual DbSet<AppUser> AppUsers { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
    }
}
