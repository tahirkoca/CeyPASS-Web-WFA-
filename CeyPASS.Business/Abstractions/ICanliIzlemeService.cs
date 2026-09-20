using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Canlı izleme ekranı giriş ve son geçişler.</summary>
    public interface ICanliIzlemeService
    {
        /// <summary>Canlı izlemede seçilebilir firmalar.</summary>
        DataTable GetFirmalar();

        /// <summary>Firma bazlı canlı izleme oturumu.</summary>
        AuthUserDTO Login(int firmaId, string user, string pass);

        /// <summary>Son kapı geçişleri.</summary>
        List<LastPassDTO> GetLastPasses(int firmaId, int take);

        /// <summary>Son yemekhane geçişleri.</summary>
        List<LastPassDTO> GetLastPassesYemekhane(int firmaId, int take);

        /// <summary>Son araç geçişleri.</summary>
        List<LastPassDTO> GetLastPassesArac(int firmaId, int take);

        /// <summary>Firmaya tanımlı canlı izleme kullanıcı adları.</summary>
        List<string> GetKullaniciAdlariByFirma(int firmaId);
    }
}
