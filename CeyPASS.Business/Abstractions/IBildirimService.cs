using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Uygulama içi bildirim oluşturma ve okuma.</summary>
    public interface IBildirimService
    {
        /// <summary>Kullanıcı veya personele bildirim ekler.</summary>
        void AddNotification(int? kullaniciId, string? personelId, string baslik, string mesaj, string tipi, int? ilgiliKayitId = null);

        /// <summary>İlgili kullanıcı/personel bildirimlerini getirir.</summary>
        List<Bildirim> GetMyNotifications(string? personelId, int? kullaniciId);

        /// <summary>Tek bildirimi okundu işaretler.</summary>
        void MarkAsRead(int bildirimId);

        /// <summary>Tüm bildirimleri okundu işaretler.</summary>
        void MarkAllAsRead(string? personelId, int? kullaniciId);

        /// <summary>Okunmamış bildirim sayısı.</summary>
        int GetUnreadCount(string? personelId, int? kullaniciId);
    }
}
