using CeyPASS.Business.Abstractions;

namespace CeyPASS.Infrastructure.Helpers
{
    public class AuthorizationHelper
    {
        private readonly IAuthorizationService _auth;
        private readonly ISessionContext _session;

        public AuthorizationHelper(ISessionContext session, IAuthorizationService auth)
        {
            _session = session;
            _auth = auth;
        }
    }
}
