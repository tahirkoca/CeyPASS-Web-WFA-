using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface ICanliIzlemeRepository
    {
        List<LastPassDTO> GetLastPasses(int firmaId, int take);
        List<LastPassDTO> GetLastPassesYemekhane(int firmaId, int take);
        AuthUserDTO Validate(int firmaId, string user, string password);
        /// <summary>Canlı izleme giriş ekranı dropdown için firma bazlı kullanıcı adlarını getirir.</summary>
        List<string> GetKullaniciAdlariByFirma(int firmaId);
    }
}
