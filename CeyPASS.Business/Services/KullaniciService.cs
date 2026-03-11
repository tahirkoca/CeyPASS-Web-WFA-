using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class KullaniciService:IKullaniciService
    {
        private readonly IKullaniciRepository _repo;

        public KullaniciService(IKullaniciRepository repo)
        {
            _repo= repo;
        }
        public Kullanici GirisYap(string kullaniciAdi, string sifre)
        {
            return _repo.KullaniciDogrula(kullaniciAdi, sifre);
        }
        public List<string> GetTumKullaniciAdlari()
        {
            return _repo.GetTumKullaniciAdlari();
        }
    }
}
