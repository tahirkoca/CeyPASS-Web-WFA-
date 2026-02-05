using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IMailRepository
    {
        Dictionary<string, List<string>> AliciGruplariniGetir();
        bool AliciEkle(string grupAdi, string emailAdresi, string adSoyad);
        bool AliciSil(int aliciId);
    }
}
