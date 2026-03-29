namespace CeyPASS.DataAccess.Abstractions
{
    public interface IPersonelWebSifreRepository
    {
        bool Dogrula(string personelId, string sifre);
        bool EkleVeyaGuncelle(string personelId, string sifre);
        string? GetSifreById(string personelId);
        void KurtarmaKoduKaydet(string personelId, string kod, System.DateTime expireTime);
        string? GetKurtarmaKodu(string personelId);
        void KurtarmaKodunuTemizle(string personelId);
    }
}
