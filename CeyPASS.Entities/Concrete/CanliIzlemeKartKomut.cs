namespace CeyPASS.Entities.Concrete
{
    /// <summary>Canlı izleme: cihazda kartı aktif/pasif yapma kuyruk kaydı.</summary>
    public class CanliIzlemeKartKomut
    {
        public int Id { get; set; }
        public int FirmaId { get; set; }
        public string PersonelId { get; set; } = "";
        public string KartNo { get; set; }
        public string Komut { get; set; } = "";
        public System.DateTime Tarih { get; set; }
        public bool OkunduMu { get; set; }
        public int? OlusturanKullaniciId { get; set; }
    }

    /// <summary><see cref="CanliIzlemeKartKomut.Komut"/> sabit değerleri.</summary>
    public static class CanliIzlemeKartKomutTurleri
    {
        public const string Aktif = "AKTIF";
        public const string Pasif = "PASIF";
    }

    public static class KartAtamaMesajlari
    {
        public static string Guncellendi(bool kisitKaldirildi) =>
            kisitKaldirildi ? "Kayıt güncellendi. Kartın cihaz kısıtı da kaldırıldı." : "Kayıt güncellendi.";
    }
}
