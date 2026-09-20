using CeyPASS.Entities.Concrete;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Oturumdaki kullanıcı, firma ve rol bilgisi.</summary>
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

        /// <summary>RolId 1 veya 2 ise yönetici kabul edilir.</summary>
        bool IsAdmin();

        /// <summary>Giriş yapmış kullanıcı DTO'su.</summary>
        AuthUserDTO CurrentUser { get; }

        /// <summary>Oturum kullanıcısını ayarlar.</summary>
        void SetCurrentUser(AuthUserDTO user);

        /// <summary>Oturum alanlarını temizler.</summary>
        void Clear();
    }
}
