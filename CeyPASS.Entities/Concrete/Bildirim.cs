using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CeyPASS.Entities.Concrete
{
    [Table("Bildirimler")]
    public class Bildirim
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? KullaniciId { get; set; }
        
        [StringLength(50)]
        public string? PersonelId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Baslik { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Mesaj { get; set; }
        
        public bool OkunduMu { get; set; }
        
        public DateTime OlusturmaTarihi { get; set; }
        
        [StringLength(50)]
        public string Tipi { get; set; }
        
        public int? IlgiliKayitId { get; set; }
    }
}
