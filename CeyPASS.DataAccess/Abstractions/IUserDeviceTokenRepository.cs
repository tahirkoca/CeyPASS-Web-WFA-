using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IUserDeviceTokenRepository
    {
        bool AddOrUpdate(UserDeviceToken token);
        void Deactivate(string fcmToken);
        List<string> GetTokensByUser(string? personelId, string? kullaniciId);
    }
}
