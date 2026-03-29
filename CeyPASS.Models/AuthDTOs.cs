namespace CeyPASS.Models.Auth
{
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public AuthUserDTO User { get; set; } = new();
    }

    public class AuthUserDTO
    {
        public int KullaniciId { get; set; }
        public int FirmaId { get; set; }
        public string? FirmaAdi { get; set; }
        public string? KullaniciAdi { get; set; }
        public string? AdSoyad { get; set; }
        public string? Rol { get; set; }
        public int? RolId { get; set; }
        public string? SicilNo { get; set; }
    }
}
