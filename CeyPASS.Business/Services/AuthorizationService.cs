using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;

namespace CeyPASS.Business.Services
{
    /// <summary>Sayfa bazlı yetki kontrollerinin uygulaması.</summary>
    public class AuthorizationService:IAuthorizationService
    {
        private readonly ISessionContext _session;
        private readonly IAuthorizationRepository _repo;

        public AuthorizationService(
            ISessionContext session, 
            IAuthorizationRepository repo)
        {
            _session = session;
            _repo = repo;
        }

        /// <inheritdoc />
        public bool Can(string sayfaAdi, string yetkiTipi)
        {
            if (_session.RolId == 1 || _session.RolId == 2)
                return true;

            // Salt personel sadece Profil sayfasındaki işlemleri yapabilir.
            if (_session.RolId == 5 && sayfaAdi == "Profil")
                return true;

            if (!_session.AktifKullaniciId.HasValue || _session.AktifKullaniciId.Value <= 0)
                return false;

            return _repo.CheckPermission(_session.AktifKullaniciId.Value, sayfaAdi, yetkiTipi);
        }
        /// <inheritdoc />
        public bool ViewAbility(string page) => Can(page, YetkiTipleri.View);
        /// <inheritdoc />
        public bool CreateAbility(string page) => Can(page, YetkiTipleri.Create);
        /// <inheritdoc />
        public bool UpdateAbility(string page) => Can(page, YetkiTipleri.Update);
        /// <inheritdoc />
        public bool DeleteAbility(string page) => Can(page, YetkiTipleri.Delete);
        /// <inheritdoc />
        public bool ExportAbility(string page) => Can(page, YetkiTipleri.Export);
        /// <inheritdoc />
        public bool ApproveAbility(string page) => Can(page, YetkiTipleri.Approve);
    }
}
