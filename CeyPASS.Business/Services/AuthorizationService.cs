using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;

namespace CeyPASS.Business.Services
{
    public class AuthorizationService:IAuthorizationService
    {
        private readonly ISessionContext _session;
        private readonly IAuthorizationRepository _repo;

        public AuthorizationService(ISessionContext session,IAuthorizationRepository repo)
        {
           _session = session;
            _repo = repo;
        }

        public bool Can(string sayfaAdi, string yetkiTipi)
        {
            if (!_session.AktifKullaniciId.HasValue || _session.AktifKullaniciId.Value <= 0)
                return false;
            if (_session.RolId == 1 || _session.RolId == 2)
                return true;
            return _repo.CheckPermission(_session.AktifKullaniciId.Value, sayfaAdi, yetkiTipi);
        }
        public bool ViewAbility(string page) => Can(page, YetkiTipleri.View);
        public bool CreateAbility(string page) => Can(page, YetkiTipleri.Create);
        public bool UpdateAbility(string page) => Can(page, YetkiTipleri.Update);
        public bool DeleteAbility(string page) => Can(page, YetkiTipleri.Delete);
        public bool ExportAbility(string page) => Can(page, YetkiTipleri.Export);
        public bool ApproveAbility(string page) => Can(page, YetkiTipleri.Approve);
    }
}
