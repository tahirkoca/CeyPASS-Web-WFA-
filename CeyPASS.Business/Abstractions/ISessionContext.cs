using CeyPASS.Entities.Concrete;

namespace CeyPASS.Business.Abstractions
{
    public interface ISessionContext
    {
        int? AktifKullaniciId { get; set; }
        int? AktifFirmaId { get; set; }
        string AdSoyad { get; set; }
        string RolAdi { get; set; }
        int? RolId { get; set; }
        AuthUserDTO CurrentUser { get; }
        void SetCurrentUser(AuthUserDTO user);
        void Clear();
    }
}
