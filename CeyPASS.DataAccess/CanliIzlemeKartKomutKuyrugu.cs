using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CeyPASS.DataAccess
{
    [Table("CanliIzlemeKartKomutKuyrugu")]
    public class CanliIzlemeKartKomutKuyrugu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int FirmaId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PersonelId { get; set; }

        [MaxLength(100)]
        public string KartNo { get; set; }

        [Required]
        [MaxLength(10)]
        public string Komut { get; set; }

        public DateTime Tarih { get; set; }

        public bool OkunduMu { get; set; }

        public int? OlusturanKullaniciId { get; set; }
    }
}
