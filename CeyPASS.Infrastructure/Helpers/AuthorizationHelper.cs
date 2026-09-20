using CeyPASS.Business.Abstractions;

namespace CeyPASS.Infrastructure.Helpers
{
    /// <summary>Oturum ve yetkilendirme servislerine erişim için yardımcı (genişletmeye hazır).</summary>
    public class AuthorizationHelper
    {
        private readonly IAuthorizationService _auth;
        private readonly ISessionContext _session;

        /// <summary>Aktif oturum ve yetki servisini bağlar.</summary>
        public AuthorizationHelper(ISessionContext session, IAuthorizationService auth)
        {
            _session = session;
            _auth = auth;
        }
    }
}
