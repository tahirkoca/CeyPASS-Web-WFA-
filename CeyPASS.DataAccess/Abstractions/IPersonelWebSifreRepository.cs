namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Personel web portal şifre işlemleri.</summary>
    public interface IPersonelWebSifreRepository
    {
        /// <summary>Dogrula işlemini gerçekleştirir.</summary>
        bool Dogrula(string personelId, string sifre);
        /// <summary>Ekle Veya Guncelle işlemini gerçekleştirir.</summary>
        bool EkleVeyaGuncelle(string personelId, string sifre);
        /// <summary>Kimliğe göre kaydı getirir.</summary>
        string? GetSifreById(string personelId);
        /// <summary>Kurtarma Kodu Kaydet işlemini gerçekleştirir.</summary>
        void KurtarmaKoduKaydet(string personelId, string kod, System.DateTime expireTime);
        /// <summary>Kurtarma Kodu sorgularını getirir.</summary>
        string? GetKurtarmaKodu(string personelId);
        /// <summary>Kurtarma Kodunu Temizle işlemini gerçekleştirir.</summary>
        void KurtarmaKodunuTemizle(string personelId);
    }
}
