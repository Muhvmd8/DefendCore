using DefendCore.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;
namespace DefendCore.Infrastructure.Presistence.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {

        }

        #region Securtiy Schema
        public DbSet<LoginAudit> LoginAudits { get; set; } = default!;
        public DbSet<BlockedIp> BlockedIps { get; set; } = default!;
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
