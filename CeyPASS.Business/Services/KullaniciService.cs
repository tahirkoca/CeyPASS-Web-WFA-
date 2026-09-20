using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    /// <summary>Kullanıcı giriş ve sorgu.</summary>
    public class KullaniciService:IKullaniciService
    {
        private readonly IKullaniciRepository _repo;

        public KullaniciService(IKullaniciRepository repo)
        {
            _repo= repo;
        }
        /// <inheritdoc />
        public Kullanici GirisYap(string kullaniciAdi, string sifre)
        {
            return _repo.KullaniciDogrula(kullaniciAdi, sifre);
        }
        /// <inheritdoc />
        public List<string> GetTumKullaniciAdlari()
        {
            return _repo.GetTumKullaniciAdlari();
        }

        /// <inheritdoc />
        public Kullanici GetByPersonelId(string personelId)
        {
            return _repo.GetByPersonelId(personelId);
        }

        /// <inheritdoc />
        public Kullanici GetByUserName(string kullaniciAdi)
        {
            return _repo.GetByUserName(kullaniciAdi);
        }
    }
}
