using CeyPASS.Business.Abstractions;
using System;
using System.Net;

namespace CeyPASS.Infrastructure.Helpers
{
    public static class LogHelper
    {
        private static ISistemLogService? _svc;
        private static ISessionContext? _session;
        private static string? _ip;
        private static string? _pc;
        private static string Ip => _ip ??= GetLocalIp() ?? "";
        private static string Pc => _pc ??= Environment.MachineName ?? "";
        public static void Configure(ISistemLogService svc, ISessionContext session)
        {
            _svc = svc;
            _session = session;
            _ip = GetLocalIp();
            _pc = Environment.MachineName;
        }
        private static int? CurrentUserId()
        {
            return _session?.AktifKullaniciId;
        }

        static LogHelper() { }

        public static void Info(string kaynak, string islem, string mesaj, string? detayJson = null, string? cid = null)
        {
            if (_svc == null) return;
            _svc.Info(CurrentUserId(), kaynak, islem, mesaj, Ip, Pc, detayJson, cid);
        }
        public static void Warn(string kaynak, string islem, string mesaj, string? detayJson = null, string? cid = null)
        {
            if (_svc == null) return;
            _svc.Warn(CurrentUserId(), kaynak, islem, mesaj, Ip, Pc, detayJson, cid);
        }
        public static void Error(string kaynak, string islem, string mesaj, Exception ex, string? detayJson = null, string? cid = null)
        {
            if (_svc == null) return;
            _svc.Error(CurrentUserId(), kaynak, islem, mesaj, Ip, Pc, ex, detayJson, cid);
        }
        private static string? GetLocalIp()
        {
            try
            {
                foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return ip.ToString();
            }
            catch { }
            return null;
        }
        public static string? Escape(string? s) => string.IsNullOrEmpty(s) ? s : s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
