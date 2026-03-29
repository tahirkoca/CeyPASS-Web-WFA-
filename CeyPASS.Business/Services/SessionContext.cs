using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;

namespace CeyPASS.Business.Services
{
    public class SessionContext : ISessionContext
    {
        private AuthUserDTO _currentUser;
        private int? _aktifKullaniciId;
        private int? _aktifFirmaId;
        private string? _aktifSicilNo;
        private string _rolAdi;

        public int? AktifKullaniciId 
        { 
            get => _currentUser != null ? (int?)CurrentUser.KullaniciId : _aktifKullaniciId; 
            set 
            {
                _aktifKullaniciId = value;
                if (_currentUser != null)
                    _currentUser.KullaniciId = value ?? 0;
            }
        }
        
        public int? AktifFirmaId 
        { 
            get => _currentUser != null ? (int?)CurrentUser.FirmaId : _aktifFirmaId; 
            set 
            {
                _aktifFirmaId = value;
                if (_currentUser != null)
                    _currentUser.FirmaId = value ?? 0;
            }
        }

        public string? AktifSicilNo
        {
            get => _currentUser?.SicilNo ?? _aktifSicilNo;
            set
            {
                _aktifSicilNo = value;
                if (_currentUser != null)
                    _currentUser.SicilNo = value;
            }
        }
        
        public string AdSoyad { get; set; }
        public string RolAdi 
        { 
            get => _currentUser?.Rol ?? _rolAdi; 
            set 
            {
                _rolAdi = value;
                if (_currentUser != null)
                    _currentUser.Rol = value;
            }
        }
        
        public int? RolId { get; set; }
        public string? UserPhotoUrl { get; set; }
        public string? UserInitials { get; set; }
        public bool? IsSupervisor { get; set; }

        public AuthUserDTO CurrentUser => _currentUser;
        public bool IsAdmin() => RolId == 1 || RolId == 2;

        public void SetCurrentUser(AuthUserDTO user)
        {
            _currentUser = user;
            if (user != null)
            {
                AdSoyad = user.KullaniciAdi;
            }
        }

        public void Clear()
        {
            _currentUser = null;
            _aktifKullaniciId = null;
            _aktifFirmaId = null;
            _aktifSicilNo = null;
            _rolAdi = null;
            AdSoyad = null;
            RolId = null;
        }
    }
}
