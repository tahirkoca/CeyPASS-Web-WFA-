using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text.Json;
using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;

namespace CeyPASS.Business.Services
{
    /// <summary>Rapor SP çalıştırma (ADO.NET — EF interceptor dışında; Action log köprüsü).</summary>
    public class RaporService : IRaporService
    {
        private readonly IRaporRepository _repo;
        private readonly ISistemLogService? _log;
        private readonly ISessionContext? _session;

        public RaporService(IRaporRepository repo, ISistemLogService? log = null, ISessionContext? session = null)
        {
            _repo = repo;
            _log = log;
            _session = session;
        }

        /// <inheritdoc />
        public List<RaporTanimi> GetirRaporlar()
        {
            return _repo.RaporlariGetir();
        }

        /// <inheritdoc />
        public IReadOnlyList<string> GetProcedureParameterNames(string procedureAdi)
        {
            return _repo.GetProcedureParameterNames(procedureAdi);
        }

        /// <inheritdoc />
        public DataTable CalistirRapor(string procedureAdi, Dictionary<string, object> parametreler)
        {
            var cid = Guid.NewGuid().ToString("N");
            try
            {
                var dt = _repo.RaporuCalistir(procedureAdi, parametreler);
                TryLog("Calistir", $"Rapor çalıştırıldı: {procedureAdi}", procedureAdi, dt?.Rows?.Count ?? 0, parametreler, cid);
                return dt;
            }
            catch (Exception ex)
            {
                TryLogError(procedureAdi, parametreler, ex, cid);
                throw;
            }
        }

        private void TryLog(string islem, string mesaj, string procedureAdi, int rowCount,
            Dictionary<string, object> parametreler, string cid)
        {
            if (_log == null) return;
            try
            {
                var detay = JsonSerializer.Serialize(new
                {
                    Procedure = procedureAdi,
                    Rows = rowCount,
                    Params = MaskParams(parametreler)
                });
                _log.Info(_session?.AktifKullaniciId, "Raporlar", islem, Truncate(mesaj, 2000),
                    GetLocalIp() ?? "", Environment.MachineName ?? "", detay, cid);
            }
            catch { /* audit asla business akışını düşürmez */ }
        }

        private void TryLogError(string procedureAdi, Dictionary<string, object> parametreler, Exception ex, string cid)
        {
            if (_log == null) return;
            try
            {
                var detay = JsonSerializer.Serialize(new
                {
                    Procedure = procedureAdi,
                    Params = MaskParams(parametreler)
                });
                _log.Error(_session?.AktifKullaniciId, "Raporlar", "Calistir",
                    Truncate($"Rapor hata: {procedureAdi}", 2000),
                    GetLocalIp() ?? "", Environment.MachineName ?? "", ex, detay, cid);
            }
            catch { }
        }

        private static Dictionary<string, object?> MaskParams(Dictionary<string, object>? parametreler)
        {
            var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            if (parametreler == null) return result;
            foreach (var kv in parametreler)
            {
                var key = kv.Key ?? "";
                var lower = key.ToLowerInvariant();
                if (lower.Contains("sifre") || lower.Contains("password") || lower.Contains("token") || lower.Contains("secret"))
                    result[key] = "***";
                else
                    result[key] = kv.Value;
            }
            return result;
        }

        private static string Truncate(string? s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= max ? s : s.Substring(0, max);
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
    }
}
