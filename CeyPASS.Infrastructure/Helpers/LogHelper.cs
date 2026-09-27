using CeyPASS.Business.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;

namespace CeyPASS.Infrastructure.Helpers
{
    /// <summary>Statik sistem log köprüsü (WFA/WPF export ve manuel Action log).</summary>
    public static class LogHelper
    {
        private static IServiceProvider? _root;
        private static ISistemLogService? _svc;
        private static ISessionContext? _session;
        private static string? _ip;
        private static string? _pc;
        private static string Ip => _ip ??= GetLocalIp() ?? "";
        private static string Pc => _pc ??= Environment.MachineName ?? "";

        /// <summary>Root DI — Web/Api’de request scope’undan kullanıcı çözümler.</summary>
        public static void Configure(IServiceProvider root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _ip = GetLocalIp();
            _pc = Environment.MachineName;
        }

        /// <summary>Log servisi ve oturum bağlamını uygulama başlangıcında kaydeder (masaüstü).</summary>
        public static void Configure(ISistemLogService svc, ISessionContext session)
        {
            _svc = svc;
            _session = session;
            _ip = GetLocalIp();
            _pc = Environment.MachineName;
        }

        /// <summary>Bilgi seviyesinde sistem logu.</summary>
        public static void Info(string kaynak, string islem, string mesaj, string? detayJson = null, string? cid = null)
        {
            TryWrite((svc, uid) => svc.Info(uid, kaynak, islem, mesaj, Ip, Pc, detayJson, cid));
        }

        /// <summary>Uyarı seviyesinde sistem logu.</summary>
        public static void Warn(string kaynak, string islem, string mesaj, string? detayJson = null, string? cid = null)
        {
            TryWrite((svc, uid) => svc.Warn(uid, kaynak, islem, mesaj, Ip, Pc, detayJson, cid));
        }

        /// <summary>Hata seviyesinde sistem logu (istisna ile).</summary>
        public static void Error(string kaynak, string islem, string mesaj, Exception ex, string? detayJson = null, string? cid = null)
        {
            TryWrite((svc, uid) => svc.Error(uid, kaynak, islem, mesaj, Ip, Pc, ex, detayJson, cid));
        }

        private static void TryWrite(Action<ISistemLogService, int?> write)
        {
            try
            {
                if (_root != null)
                {
                    IServiceScope? owned = null;
                    try
                    {
                        var sp = ResolveProvider(_root, out owned);
                        var svc = sp.GetService<ISistemLogService>();
                        if (svc == null) return;
                        var uid = sp.GetService<ISessionContext>()?.AktifKullaniciId;
                        write(svc, uid);
                    }
                    finally
                    {
                        owned?.Dispose();
                    }
                    return;
                }

                if (_svc == null) return;
                write(_svc, _session?.AktifKullaniciId);
            }
            catch
            {
                /* log asla iş akışını düşürmez */
            }
        }

        private static IServiceProvider ResolveProvider(IServiceProvider root, out IServiceScope? owned)
        {
            owned = null;
            var http = root.GetService<IHttpContextAccessor>();
            if (http?.HttpContext?.RequestServices != null)
                return http.HttpContext.RequestServices;

            owned = root.CreateScope();
            return owned.ServiceProvider;
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
