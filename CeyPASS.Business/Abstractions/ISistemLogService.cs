using CeyPASS.Entities.Concrete;
using System;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Uygulama sistem log kaydı.</summary>
    public interface ISistemLogService
    {
        /// <summary>Ham log entity yazar.</summary>
        void Logla(SistemLog log);

        /// <summary>Bilgi seviyesi log.</summary>
        void Info(int? uid, string kaynak, string islem, string mesaj, string ip, string pc, string? detayJson = null, string? cid = null);

        /// <summary>Uyarı seviyesi log.</summary>
        void Warn(int? uid, string kaynak, string islem, string mesaj, string ip, string pc, string? detayJson = null, string? cid = null);

        /// <summary>Hata seviyesi log (istisna ile).</summary>
        void Error(int? uid, string kaynak, string islem, string mesaj, string ip, string pc, Exception ex, string? detayJson = null, string? cid = null);
    }
}
