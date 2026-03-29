using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IBildirimService
    {
        void AddNotification(int? kullaniciId, string? personelId, string baslik, string mesaj, string tipi, int? ilgiliKayitId = null);
        List<Bildirim> GetMyNotifications(string? personelId, int? kullaniciId);
        void MarkAsRead(int bildirimId);
        void MarkAllAsRead(string? personelId, int? kullaniciId);
        int GetUnreadCount(string? personelId, int? kullaniciId);
    }
}
