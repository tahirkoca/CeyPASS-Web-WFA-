using System.Collections.Generic;
using System.Threading.Tasks;

namespace CeyPASS.Business.Abstractions
{
    public interface IPushNotificationService
    {
        Task SendPushToUserAsync(string? personelId, string? kullaniciId, string title, string body, object? data = null);
    }
}
