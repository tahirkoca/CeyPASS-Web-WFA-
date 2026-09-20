using CeyPASS.Business.Abstractions;
using System;
using System.Net;

namespace CeyPASS.Infrastructure.Helpers
{
    /// <summary>Statik sistem log köprüsü (WFA/WPF ve export senaryoları).</summary>
    public static class LogHelper
    {
        private static ISistemLogService? _svc;
        private static ISessionContext? _session;
        private static string? _ip;
        private static string? _pc;
        private static string Ip => _ip ??= GetLocalIp() ?? "";
        private static string Pc => _pc ??= Environment.MachineName ?? "";

        /// <summary>Log servisi ve oturum bağlamını uygulama başlangıcında kaydeder.</summary>
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

        /// <summary>Bilgi seviyesinde sistem logu.</summary>
        public static void Info(string kaynak, string islem, string mesaj, string? detayJson = null, string? cid = null)
        {
            if (_svc == null) return;
            _svc.Info(CurrentUserId(), kaynak, islem, mesaj, Ip, Pc, detayJson, cid);
        }

        /// <summary>Uyarı seviyesinde sistem logu.</summary>
        public static void Warn(string kaynak, string islem, string mesaj, string? detayJson = null, string? cid = null)
        {
            if (_svc == null) return;
            _svc.Warn(CurrentUserId(), kaynak, islem, mesaj, Ip, Pc, detayJson, cid);
        }

        /// <summary>Hata seviyesinde sistem logu (istisna ile).</summary>
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

        /// <summary>JSON detay alanları için basit kaçış.</summary>
        public static string? Escape(string? s) => string.IsNullOrEmpty(s) ? s : s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
