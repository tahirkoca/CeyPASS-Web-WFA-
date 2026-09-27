using System;
using System.Threading;

namespace CeyPASS.Entities.Audit
{
    /// <summary>
    /// SaveChanges sırasında CommandInterceptor’ı ve SistemLog INSERT recursion’ını bastırmak için.
    /// </summary>
    public static class AuditWriteSuppress
    {
        private static readonly AsyncLocal<int> Depth = new();

        public static bool IsActive => Depth.Value > 0;

        public static IDisposable Enter()
        {
            Depth.Value++;
            return new Releaser();
        }

        private sealed class Releaser : IDisposable
        {
            private bool _disposed;
            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                if (Depth.Value > 0) Depth.Value--;
            }
        }
    }
}
