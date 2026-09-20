using System;

namespace CeyPASS.Entities.Concrete
{
    /// <summary>Personelin birincil üst yetkili sicil eşlemesi (izin onayı).</summary>
    public class UstYetkili
    {
        public string PersonelId { get; set; } = "";
        public string UstYetkiliPersonelId { get; set; } = "";
        public DateTime OlusturmaTarihi { get; set; }
    }
}

