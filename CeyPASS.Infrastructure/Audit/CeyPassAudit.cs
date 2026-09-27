using System;
using Audit.Core;
using CeyPASS.Infrastructure.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CeyPASS.Infrastructure.Audit
{
    /// <summary>Host startup’ta Audit.NET → SistemLoglari köprüsünü kurar.</summary>
    public static class CeyPassAudit
    {
        /// <summary>
        /// Uygulama başlangıcında bir kez çağrılır (WFA/WPF/Web/Api).
        /// </summary>
        public static void Configure(IServiceProvider rootServices, string clientName)
        {
            if (rootServices == null) throw new ArgumentNullException(nameof(rootServices));
            var client = string.IsNullOrWhiteSpace(clientName) ? "Unknown" : clientName.Trim();

            LogHelper.Configure(rootServices);

            global::Audit.EntityFramework.Configuration.Setup()
                .ForAnyContext(config => config
                    .IncludeEntityObjects(false)
                    .AuditEventType("{context}:{database}"))
                .UseOptOut()
                .IgnoreAny(t => string.Equals(t.Name, "SistemLoglari", StringComparison.OrdinalIgnoreCase));

            global::Audit.Core.Configuration.Setup()
                .UseCustomProvider(new SistemLogAuditDataProvider(rootServices, client));
        }
    }
}
