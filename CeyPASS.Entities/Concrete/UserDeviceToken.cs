using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CeyPASS.Entities.Concrete
{
    [Table("MobilUygulamaTokenleri")]
    public class UserDeviceToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(50)]
        public string? KullaniciId { get; set; }

        [StringLength(50)]
        public string? PersonelId { get; set; }

        [Required]
        public string FCMToken { get; set; }

        [StringLength(50)]
        public string? DeviceType { get; set; }

        public DateTime LastUpdated { get; set; }

        public bool IsActive { get; set; }
    }
}
