using System;
using System.ComponentModel.DataAnnotations;

namespace CeyPASS.Web.Models.Admin
{
    /// <summary>
    /// Admin paneli - Güncelleme bildirimi mail formu.
    /// WinForms ucGuncellemeMailEkrani ile aynı veri yapısı.
    /// </summary>
    public class GuncellemeMailViewModel
    {
        [Required(ErrorMessage = "Versiyon numarası giriniz.")]
        [Display(Name = "Versiyon No")]
        public string VersiyonNumarasi { get; set; } = "1.0.0";

        [Required]
        [Display(Name = "Yayın Tarihi")]
        [DataType(DataType.Date)]
        public DateTime YayinTarihi { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Güncelleme tipi seçiniz.")]
        [Display(Name = "Güncelleme Tipi")]
        public string GuncellemeTipi { get; set; } = "Minor";

        /// <summary>Her satır bir madde (Yeni Özellikler).</summary>
        [Display(Name = "Yeni Özellikler")]
        public string YeniOzelliklerMetni { get; set; }

        /// <summary>Her satır bir madde (İyileştirmeler).</summary>
        [Display(Name = "İyileştirmeler")]
        public string IyilestirmelerMetni { get; set; }

        /// <summary>Her satır bir madde (Hata Düzeltmeleri).</summary>
        [Display(Name = "Hata Düzeltmeleri")]
        public string HataDuzeltmeleriMetni { get; set; }

        /// <summary>Her satır bir madde (Kritik Değişiklikler).</summary>
        [Display(Name = "Kritik Değişiklikler")]
        public string KritikDegisikliklerMetni { get; set; }

        [Display(Name = "Ek Notlar")]
        public string EkNotlar { get; set; } = "Bu güncelleme yapıldıktan sonra uygulamanın yeniden başlatılması gerekmektedir.";
    }
}
