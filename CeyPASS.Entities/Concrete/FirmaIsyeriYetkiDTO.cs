
namespace CeyPASS.Entities.Concrete
{
    /// <summary>Kullanıcının görebildiği firma; IsyeriId null ise tüm işyerleri.</summary>
    public class FirmaIsyeriYetkiDTO
    {
        public int FirmaId { get; set; }
        public int? IsyeriId { get; set; }
    }
}
