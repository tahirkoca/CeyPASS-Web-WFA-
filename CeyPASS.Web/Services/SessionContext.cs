using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace CeyPASS.Web.Services
{
    /// <summary>
    /// ASP.NET Core için SessionContext implementasyonu
    /// </summary>
    public class SessionContext : ISessionContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private AuthUserDTO? _currentUser;

        public SessionContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            LoadFromSession();
        }

        private void LoadFromSession()
        {
            var session = _httpContextAccessor?.HttpContext?.Session;
            if (session != null && session.IsAvailable)
            {
                var userJson = session.GetString("CurrentUser");
                if (!string.IsNullOrEmpty(userJson))
                {
                    try
                    {
                        _currentUser = JsonSerializer.Deserialize<AuthUserDTO>(userJson);
                    }
                    catch
                    {
                        _currentUser = null;
                    }
                }
            }
        }

        private void SaveToSession()
        {
            var session = _httpContextAccessor?.HttpContext?.Session;
            if (session != null && session.IsAvailable)
            {
                if (_currentUser != null)
                {
                    var userJson = JsonSerializer.Serialize(_currentUser);
                    session.SetString("CurrentUser", userJson);
                }
                else
                {
                    session.Remove("CurrentUser");
                }
            }
        }

        public int? AktifKullaniciId 
        { 
            get => CurrentUser?.KullaniciId; 
            set 
            {
                if (_currentUser != null)
                    _currentUser.KullaniciId = value ?? 0;
                SaveToSession();
            }
        }
        
        public int? AktifFirmaId 
        { 
            get => CurrentUser?.FirmaId; 
            set 
            {
                if (_currentUser != null)
                    _currentUser.FirmaId = value ?? 0;
                SaveToSession();
            }
        }
        
        public string? AdSoyad { get; set; }
        public string RolAdi 
        { 
            get => CurrentUser?.Rol; 
            set 
            {
                if (_currentUser != null)
                    _currentUser.Rol = value;
                SaveToSession();
            }
        }
        
        public int? RolId
        {
            get => CurrentUser?.RolId;
            set
            {
                if (_currentUser != null)
                {
                    _currentUser.RolId = value;
                    SaveToSession();
                }
            }
        }

        public AuthUserDTO? CurrentUser => _currentUser;

        public void SetCurrentUser(AuthUserDTO user)
        {
            _currentUser = user;
            AdSoyad = user?.AdSoyad ?? user?.KullaniciAdi;
            SaveToSession();
        }

        public void Clear()
        {
            _currentUser = null;
            AdSoyad = null;
            RolId = null;
            SaveToSession();
            
            var session = _httpContextAccessor?.HttpContext?.Session;
            if (session != null && session.IsAvailable)
            {
                session.Remove("CurrentUser");
            }
        }
    }
}
