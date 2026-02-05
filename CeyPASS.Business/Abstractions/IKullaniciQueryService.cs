using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IKullaniciQueryService
    {
        List<int> GetFirmayaAitIsyeriIdleri(int firmaId);
    }
}
