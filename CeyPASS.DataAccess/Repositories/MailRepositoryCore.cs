using CeyPASS.DataAccess.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class MailRepositoryCore : IMailRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public MailRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public Dictionary<string, List<string>> AliciGruplariniGetir()
        {
            var query = _context.SistemMailAlicilari
                .Where(x => x.Aktif == true);

            var result = query
                .GroupBy(x => x.GrupAdi)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.EmailAdresi).ToList()
                );

            return result;
        }

        public bool AliciEkle(string grupAdi, string emailAdresi, string adSoyad)
        {
            var entity = new CeyPASS.DataAccess.SistemMailAlicilari
            {
                GrupAdi = grupAdi,
                EmailAdresi = emailAdresi,
                AdSoyad = adSoyad,
                Aktif = true
            };

            _context.SistemMailAlicilari.Add(entity);
            return _context.SaveChanges() > 0;
        }

        public bool AliciSil(int aliciId)
        {
            var entity = _context.SistemMailAlicilari
                .FirstOrDefault(x => x.Id == aliciId);

            if (entity == null)
                return false;

            entity.Aktif = false;
            return _context.SaveChanges() > 0;
        }
    }
}
