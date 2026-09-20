using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Kullanıcı/firma ilişkili salt okunur sorgular.</summary>
    public interface IKullaniciQueryService
    {
        /// <summary>Firmaya bağlı işyeri kimliklerini döner.</summary>
        List<int> GetFirmayaAitIsyeriIdleri(int firmaId);
    }
}
