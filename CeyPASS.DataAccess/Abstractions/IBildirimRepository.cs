using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Uygulama bildirimlerinin okuma ve güncelleme işlemleri.</summary>
    public interface IBildirimRepository
    {
        /// <summary>Ekle işlemini gerçekleştirir.</summary>
        void Ekle(Bildirim bildirim);
        /// <summary>For User sorgularını getirir.</summary>
        List<Bildirim> GetForUser(string? personelId, int? kullaniciId);
        /// <summary>Mark As Read işlemini gerçekleştirir.</summary>
        void MarkAsRead(int bildirimId);
        /// <summary>Mark All As Read işlemini gerçekleştirir.</summary>
        void MarkAllAsRead(string? personelId, int? kullaniciId);
        /// <summary>Unread Count sorgularını getirir.</summary>
        int GetUnreadCount(string? personelId, int? kullaniciId);
    }
}
