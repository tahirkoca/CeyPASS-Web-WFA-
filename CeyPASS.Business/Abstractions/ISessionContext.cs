using CeyPASS.Entities.Concrete;

namespace CeyPASS.Business.Abstractions
{
    public interface ISessionContext
    {
        int? AktifKullaniciId { get; set; }
        int? AktifFirmaId { get; set; }
        string? AktifSicilNo { get; set; }
        string AdSoyad { get; set; }
        string RolAdi { get; set; }
        int? RolId { get; set; }
        string? UserPhotoUrl { get; set; }
        string? UserInitials { get; set; }
        bool? IsSupervisor { get; set; }
        bool IsAdmin();
        AuthUserDTO CurrentUser { get; }
        void SetCurrentUser(AuthUserDTO user);
        void Clear();
    }
}
