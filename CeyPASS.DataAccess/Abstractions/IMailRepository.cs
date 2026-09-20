using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Sistem e-posta alıcı ve gönderim yardımcıları.</summary>
    public interface IMailRepository
    {
        /// <summary>Alici Gruplarini Getir işlemini gerçekleştirir.</summary>
        Dictionary<string, List<string>> AliciGruplariniGetir();
        /// <summary>Alici Ekle işlemini gerçekleştirir.</summary>
        bool AliciEkle(string grupAdi, string emailAdresi, string adSoyad);
        /// <summary>Alici Sil işlemini gerçekleştirir.</summary>
        bool AliciSil(int aliciId);
    }
}
