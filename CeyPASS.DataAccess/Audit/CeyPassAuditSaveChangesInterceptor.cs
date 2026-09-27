using System;
using System.Threading;
using System.Threading.Tasks;
using Audit.EntityFramework;
using CeyPASS.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CeyPASS.DataAccess.Audit
{
    /// <summary>
    /// SaveChanges entity audit; süresince CommandInterceptor bastırılır (çift log yok).
    /// </summary>
    public sealed class CeyPassAuditSaveChangesInterceptor : AuditSaveChangesInterceptor
    {
        private IDisposable? _suppress;

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            _suppress = AuditWriteSuppress.Enter();
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            _suppress = AuditWriteSuppress.Enter();
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            try
            {
                return base.SavedChanges(eventData, result);
            }
            finally
            {
                _suppress?.Dispose();
                _suppress = null;
            }
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SavedChangesAsync(eventData, result, cancellationToken);
            }
            finally
            {
                _suppress?.Dispose();
                _suppress = null;
            }
        }

        public override void SaveChangesFailed(DbContextErrorEventData eventData)
        {
            try
            {
                base.SaveChangesFailed(eventData);
            }
            finally
            {
                _suppress?.Dispose();
                _suppress = null;
            }
        }

        public override async Task SaveChangesFailedAsync(
            DbContextErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await base.SaveChangesFailedAsync(eventData, cancellationToken);
            }
            finally
            {
                _suppress?.Dispose();
                _suppress = null;
            }
        }
    }
}
