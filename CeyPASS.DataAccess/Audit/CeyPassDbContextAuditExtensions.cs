using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.DataAccess.Audit
{
    /// <summary>DbContext’e SaveChanges + ExecuteSqlRaw audit interceptor ekler.</summary>
    public static class CeyPassDbContextAuditExtensions
    {
        public static DbContextOptionsBuilder AddCeyPassAuditing(this DbContextOptionsBuilder options)
        {
            options.AddInterceptors(
                new CeyPassAuditSaveChangesInterceptor(),
                new CeyPassAuditCommandInterceptor());
            return options;
        }

        public static DbContextOptionsBuilder<TContext> AddCeyPassAuditing<TContext>(
            this DbContextOptionsBuilder<TContext> options)
            where TContext : DbContext
        {
            options.AddInterceptors(
                new CeyPassAuditSaveChangesInterceptor(),
                new CeyPassAuditCommandInterceptor());
            return options;
        }
    }
}
