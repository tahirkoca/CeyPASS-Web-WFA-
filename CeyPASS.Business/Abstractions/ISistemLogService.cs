using CeyPASS.Entities.Concrete;
using System;

namespace CeyPASS.Business.Abstractions
{
    public interface ISistemLogService
    {
        void Logla(SistemLog log);
        void Info(int? uid, string kaynak, string islem, string mesaj, string ip, string pc, string? detayJson = null, string? cid = null);
        void Warn(int? uid, string kaynak, string islem, string mesaj, string ip, string pc, string? detayJson = null, string? cid = null);
        void Error(int? uid, string kaynak, string islem, string mesaj, string ip, string pc, Exception ex, string? detayJson = null, string? cid = null);
    }
}
