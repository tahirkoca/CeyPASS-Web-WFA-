using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IBildirimRepository
    {
        void Ekle(Bildirim bildirim);
        List<Bildirim> GetForUser(string? personelId, int? kullaniciId);
        void MarkAsRead(int bildirimId);
        void MarkAllAsRead(string? personelId, int? kullaniciId);
        int GetUnreadCount(string? personelId, int? kullaniciId);
    }
}
