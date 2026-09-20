using System;
using System.Collections.Generic;

namespace CeyPASS.Entities.Concrete
{
    /// <summary>Aynı TC altında ana sicil ile hedef sicil bağlantısı (kıdem/izin aktarımı).</summary>
    public sealed class CokluSicilBaglantiDTO
    {
        public string TCKimlikNo { get; set; } = "";
        public int AnaPersonelId { get; set; }
        public int HedefPersonelId { get; set; }
        public string? HedefAdSoyad { get; set; }
        public int? FirmaId { get; set; }
        public string? FirmaAdi { get; set; }
        public int? SirketId { get; set; }
        public string? IsyeriAdi { get; set; }
        public int? BolumId { get; set; }
        public string? BolumAdi { get; set; }
        public DateTime? IseGirisTarihi { get; set; }
        public DateTime? IstenCikisTarihi { get; set; }
        /// <summary>İzin/kıdem aktarımında kullanılan gün sayısı.</summary>
        public int AktarimGunSayisi { get; set; } = 1;
        public bool AktifMi { get; set; } = true;
        public string? Aciklama { get; set; }
        public DateTime OlusturmaZamani { get; set; }
        public int OlusturanKullaniciId { get; set; }
        public DateTime? GuncellemeZamani { get; set; }
        public int? GuncelleyenKullaniciId { get; set; }
    }

    /// <summary>Çoklu sicil hedef seçim listesi satırı.</summary>
    public sealed class CokluSicilHedefAdayDTO
    {
        public int PersonelId { get; set; }
        public string AdSoyad { get; set; } = "";
        public int? FirmaId { get; set; }
        public string? FirmaAdi { get; set; }
        public int? IsyeriId { get; set; }
        public string? IsyeriAdi { get; set; }
        public int? BolumId { get; set; }
        public string? BolumAdi { get; set; }
        public DateTime? IseGirisTarihi { get; set; }
        public DateTime? IstenCikisTarihi { get; set; }
        public bool PuantajYapilirMi { get; set; }
        public bool ZatenBagli { get; set; }
        public bool BagliAktif { get; set; }
        public int? BagliAnaPersonelId { get; set; }
        public bool SecilebilirMi { get; set; } = true;
        public string? EngelMesaji { get; set; }
    }

    /// <summary>Personelin çoklu sicil rol özeti (ana/hedef).</summary>
    public sealed class CokluSicilOzetDTO
    {
        public bool IsAnaSicil { get; set; }
        public bool IsHedefSicil { get; set; }
        public int AktifHedefSayisi { get; set; }
        public int? AnaPersonelId { get; set; }
    }

    /// <summary>Çoklu sicil bağlantısı oluşturma/güncelleme isteği.</summary>
    public sealed class CokluSicilUpsertRequest
    {
        public int HedefPersonelId { get; set; }
        public int? FirmaId { get; set; }
        public int? SirketId { get; set; }
        public int? BolumId { get; set; }
        public DateTime? IseGirisTarihi { get; set; }
        public DateTime? IstenCikisTarihi { get; set; }
        public int AktarimGunSayisi { get; set; } = 1;
        public string? Aciklama { get; set; }
        public bool AktifMi { get; set; } = true;
    }
}
