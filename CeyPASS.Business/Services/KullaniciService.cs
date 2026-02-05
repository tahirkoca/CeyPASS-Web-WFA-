using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;

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
    }
}
