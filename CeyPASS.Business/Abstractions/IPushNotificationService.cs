using System.Collections.Generic;
using System.Threading.Tasks;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>FCM push bildirim gönderimi.</summary>
    public interface IPushNotificationService
    {
        /// <summary>Personel veya kullanıcı kimliğine push gönderir.</summary>
        Task SendPushToUserAsync(string? personelId, string? kullaniciId, string title, string body, object? data = null);
    }
}
