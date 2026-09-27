using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Audit.EntityFramework.Interceptors;
using CeyPASS.Entities.Audit;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CeyPASS.DataAccess.Audit
{
    /// <summary>
    /// ExecuteSqlRaw / NonQuery audit; SaveChanges ve SistemLoglari yazımlarını atlar; Reader yok.
    /// </summary>
    public sealed class CeyPassAuditCommandInterceptor : AuditCommandInterceptor
    {
        public CeyPassAuditCommandInterceptor()
        {
            ExcludeReaderEvents = true;
            ExcludeScalarEvents = true;
            ExcludeNonQueryEvents = false;
            LogParameterValues = true;
            IncludeReaderResults = false;
        }

        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result)
        {
            if (ShouldSkip(command))
                return result;
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (ShouldSkip(command))
                return new ValueTask<InterceptionResult<int>>(result);
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }

        private static bool ShouldSkip(DbCommand command)
        {
            if (AuditWriteSuppress.IsActive)
                return true;
            var sql = command?.CommandText;
            if (string.IsNullOrEmpty(sql))
                return true;
            if (sql.IndexOf("SistemLoglari", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            return false;
        }
    }
}
