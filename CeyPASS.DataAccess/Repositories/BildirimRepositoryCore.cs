using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class BildirimRepositoryCore : IBildirimRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public BildirimRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public void Ekle(Bildirim bildirim)
        {
            _context.Bildirimler.Add(bildirim);
            _context.SaveChanges();
        }

        public List<Bildirim> GetForUser(string? personelId, int? kullaniciId)
        {
            var q = _context.Bildirimler.AsQueryable();

            if (!string.IsNullOrWhiteSpace(personelId) && kullaniciId.HasValue)
                q = q.Where(b => b.PersonelId == personelId || b.KullaniciId == kullaniciId.Value);
            else if (!string.IsNullOrWhiteSpace(personelId))
                q = q.Where(b => b.PersonelId == personelId);
            else if (kullaniciId.HasValue)
                q = q.Where(b => b.KullaniciId == kullaniciId.Value);

            return q.OrderByDescending(b => b.OlusturmaTarihi).ToList();
        }

        public void MarkAsRead(int bildirimId)
        {
            var b = _context.Bildirimler.Find(bildirimId);
            if (b != null && !b.OkunduMu)
            {
                b.OkunduMu = true;
                _context.SaveChanges();
            }
        }

        public int GetUnreadCount(string? personelId, int? kullaniciId)
        {
            var q = _context.Bildirimler.Where(b => !b.OkunduMu);

            if (!string.IsNullOrWhiteSpace(personelId) && kullaniciId.HasValue)
                q = q.Where(b => b.PersonelId == personelId || b.KullaniciId == kullaniciId.Value);
            else if (!string.IsNullOrWhiteSpace(personelId))
                q = q.Where(b => b.PersonelId == personelId);
            else if (kullaniciId.HasValue)
                q = q.Where(b => b.KullaniciId == kullaniciId.Value);
            else return 0; // İkisi de yoksa 0 (kimliği belirsiz)

            return q.Count();
        }

        public void MarkAllAsRead(string? personelId, int? kullaniciId)
        {
            var q = _context.Bildirimler.Where(b => !b.OkunduMu);

            if (!string.IsNullOrWhiteSpace(personelId) && kullaniciId.HasValue)
                q = q.Where(b => b.PersonelId == personelId || b.KullaniciId == kullaniciId.Value);
            else if (!string.IsNullOrWhiteSpace(personelId))
                q = q.Where(b => b.PersonelId == personelId);
            else if (kullaniciId.HasValue)
                q = q.Where(b => b.KullaniciId == kullaniciId.Value);
            else return;

            var unread = q.ToList();
            foreach (var b in unread)
            {
                b.OkunduMu = true;
            }
            _context.SaveChanges();
        }
    }
}
