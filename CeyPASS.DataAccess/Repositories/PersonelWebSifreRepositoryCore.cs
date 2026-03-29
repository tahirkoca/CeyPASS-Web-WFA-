using CeyPASS.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class PersonelWebSifreRepositoryCore : IPersonelWebSifreRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public PersonelWebSifreRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public bool Dogrula(string personelId, string sifre)
        {
            const string sql = @"
SELECT TOP 1 1 AS Value
FROM dbo.PersonelWebSifreler
WHERE PersonelId = {0} AND Sifre = {1}";

            return _context.Database.SqlQueryRaw<int>(sql, personelId, sifre).Any();
        }

        public bool EkleVeyaGuncelle(string personelId, string sifre)
        {
            const string sql = @"
IF EXISTS (SELECT 1 FROM dbo.PersonelWebSifreler WHERE PersonelId = {0})
    UPDATE dbo.PersonelWebSifreler SET Sifre = {1}, KurtarmaKodu = NULL, Kullanildi = 0 WHERE PersonelId = {0}
ELSE
    INSERT INTO dbo.PersonelWebSifreler (PersonelId, Sifre) VALUES ({0}, {1})";

            return _context.Database.ExecuteSqlRaw(sql, personelId, sifre) > 0;
        }

        public void KurtarmaKoduKaydet(string personelId, string kod, System.DateTime expireTime)
        {
            const string sql = @"
IF EXISTS (SELECT 1 FROM dbo.PersonelWebSifreler WHERE PersonelId = {0})
    UPDATE dbo.PersonelWebSifreler SET KurtarmaKodu = {1}, SonKullanmaZamani = {2}, Kullanildi = 0 WHERE PersonelId = {0}
ELSE
    INSERT INTO dbo.PersonelWebSifreler (PersonelId, KurtarmaKodu, SonKullanmaZamani, Kullanildi) VALUES ({0}, {1}, {2}, 0)";

            _context.Database.ExecuteSqlRaw(sql, personelId, kod, expireTime);
        }

        public string? GetKurtarmaKodu(string personelId)
        {
            const string sql = "SELECT KurtarmaKodu FROM dbo.PersonelWebSifreler WHERE PersonelId = {0} AND SonKullanmaZamani > GETDATE() AND Kullanildi = 0";
            return _context.Database.SqlQueryRaw<string>(sql, personelId).FirstOrDefault();
        }

        public void KurtarmaKodunuTemizle(string personelId)
        {
            const string sql = "UPDATE dbo.PersonelWebSifreler SET Kullanildi = 1 WHERE PersonelId = {0}";
            _context.Database.ExecuteSqlRaw(sql, personelId);
        }

        public string? GetSifreById(string personelId)
        {
            const string sql = "SELECT TOP 1 Sifre FROM dbo.PersonelWebSifreler WHERE PersonelId = {0}";
            return _context.Database.SqlQueryRaw<string>(sql, personelId).FirstOrDefault();
        }
    }
}
