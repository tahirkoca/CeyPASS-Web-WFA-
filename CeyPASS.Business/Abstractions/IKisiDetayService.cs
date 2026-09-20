using CeyPASS.Entities.Concrete;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Personel detay ekranı verisi.</summary>
    public interface IKisiDetayService
    {
        /// <summary>Personel kimliğine göre detay DTO.</summary>
        KisiDetayDTO GetDetay(int kisiId);
    }
}
