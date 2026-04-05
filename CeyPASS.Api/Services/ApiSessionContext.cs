using System.Security.Claims;
using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.AspNetCore.Http;

namespace CeyPASS.Api.Services
{
    public class ApiSessionContext : ISessionContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiSessionContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public int? AktifKullaniciId
        {
            get => int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
            set => throw new System.NotSupportedException("API session context does not support direct set. It is derived from JWT claims.");
        }

        public int? AktifFirmaId
        {
            get => int.TryParse(User?.FindFirst("FirmaId")?.Value, out var id) ? id : null;
            set => throw new System.NotSupportedException("API session context does not support direct set. It is derived from JWT claims.");
        }

        public string? AktifSicilNo
        {
            get => User?.FindFirst("SicilNo")?.Value;
            set => throw new System.NotSupportedException("API session context does not support direct set. It is derived from JWT claims.");
        }

        public string AdSoyad
        {
            get => User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
            set => throw new System.NotSupportedException("API session context does not support direct set. It is derived from JWT claims.");
        }

        public string RolAdi
        {
            get => User?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            set => throw new System.NotSupportedException("API session context does not support direct set. It is derived from JWT claims.");
        }

        public int? RolId
        {
            get => int.TryParse(User?.FindFirst("RolId")?.Value, out var id) ? id : null;
            set => throw new System.NotSupportedException("API session context does not support direct set. It is derived from JWT claims.");
        }

        public string? UserPhotoUrl { get; set; }
        public string? UserInitials { get; set; }
        public bool? IsSupervisor { get; set; }

        // Keep consistent with server-side SessionContext (RolId 1 or 2 are admin-level)
        public bool IsAdmin() => RolId == 1 || RolId == 2 || RolAdi == "Admin";

        public AuthUserDTO CurrentUser => new AuthUserDTO
        {
            KullaniciId = AktifKullaniciId ?? 0,
            FirmaId = AktifFirmaId ?? 0,
            KullaniciAdi = User?.Identity?.Name,
            AdSoyad = AdSoyad,
            Rol = RolAdi,
            RolId = RolId,
            SicilNo = AktifSicilNo
        };

        public void SetCurrentUser(AuthUserDTO user) { /* Token-based auth set info from login */ }
        public void Clear() { /* Token-based auth sign out on client side */ }
    }
}
