using CeyPASS.Entities.Concrete;

namespace CeyPASS.Business.Abstractions
{
    public interface IKisiDetayService
    {
        KisiDetayDTO GetDetay(int kisiId);
    }
}
