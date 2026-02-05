using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;

namespace CeyPASS.Business.Services
{
    public class SistemLogService:ISistemLogService
    {
        private readonly ISistemLogRepository _repo;

        public SistemLogService(ISistemLogRepository repo)
        {
            _repo = repo;
        }
        public void Logla(SistemLog log) => _repo.Insert(log);
        public void Info(int? uid, string kaynak, string islem, string mesaj, string ip, string pc, string? detayJson = null, string? cid = null)=> _repo.Insert(new SistemLog { KullaniciId = uid, IslemTuru = IslemTuru.Info, Kaynak = kaynak, Islem = islem, Mesaj = mesaj, IpAdres = ip, BilgisayarAdi = pc, DetayJson = detayJson, KorelasyonId = cid });
        public void Warn(int? uid, string kaynak, string islem, string mesaj, string ip, string pc, string? detayJson = null, string? cid = null)=> _repo.Insert(new SistemLog { KullaniciId = uid, IslemTuru = IslemTuru.Warn, Kaynak = kaynak, Islem = islem, Mesaj = mesaj, IpAdres = ip, BilgisayarAdi = pc, DetayJson = detayJson, KorelasyonId = cid });
        public void Error(int? uid, string kaynak, string islem, string mesaj, string ip, string pc, Exception ex, string? detayJson = null, string? cid = null)=> _repo.Insert(new SistemLog { KullaniciId = uid, IslemTuru = IslemTuru.Error, Kaynak = kaynak, Islem = islem, Mesaj = mesaj, IpAdres = ip, BilgisayarAdi = pc, DetayJson = detayJson, KorelasyonId = cid, HataMesaji = ex?.ToString() });
    }
}
