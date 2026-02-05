using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Web.Models.Admin
{
    /// <summary>
    /// Admin paneli sekmeli görünüm için tüm veri setleri.
    /// </summary>
    public class AdminPanelViewModel
    {
        public List<Firma> Firmalar { get; set; } = new List<Firma>();
        public List<IsyeriItem> Isyeriler { get; set; } = new List<IsyeriItem>();
        public List<CihazListDTO> Cihazlar { get; set; } = new List<CihazListDTO>();
        public List<DepartmanListDTO> Departmanlar { get; set; } = new List<DepartmanListDTO>();
        public List<PozisyonListDTO> Pozisyonlar { get; set; } = new List<PozisyonListDTO>();
        public List<ResmiTatilDTO> ResmiTatiller { get; set; } = new List<ResmiTatilDTO>();
        public List<LookupItem> CalismaStatuleri { get; set; } = new List<LookupItem>();
        public List<CalismaSekli> CalismaSekilleri { get; set; } = new List<CalismaSekli>();
        /// <summary>Açık kalacak sekme (firma, isyeri, cihaz, departman, pozisyon, resmitatil, calismastatu, vardiya).</summary>
        public string AktifTab { get; set; } = "firma";
    }
}
