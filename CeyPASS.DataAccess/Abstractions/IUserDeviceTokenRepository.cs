using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Mobil uygulama push token kayıtları.</summary>
    public interface IUserDeviceTokenRepository
    {
        /// <summary>Add Or Update işlemini gerçekleştirir.</summary>
        bool AddOrUpdate(UserDeviceToken token);
        /// <summary>Deactivate işlemini gerçekleştirir.</summary>
        void Deactivate(string fcmToken);
        /// <summary>Tokens By User sorgularını getirir.</summary>
        List<string> GetTokensByUser(string? personelId, string? kullaniciId);
    }
}
