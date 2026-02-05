using System.ComponentModel.DataAnnotations.Schema;

namespace CeyPASS.Entities.Concrete
{
    public class LookupItem
    {
        public int Id { get; set; }
        public string Ad { get; set; }

        [NotMapped]
        public object Tag { get; set; }

        public override string ToString() => Ad;
    }
}
