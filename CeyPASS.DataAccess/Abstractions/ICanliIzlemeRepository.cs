using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Canlı geçiş izleme ve kimlik doğrulama sorguları.</summary>
    public interface ICanliIzlemeRepository
    {
        /// <summary>Last Passes sorgularını getirir.</summary>
        List<LastPassDTO> GetLastPasses(int firmaId, int take);
        /// <summary>Last Passes Yemekhane sorgularını getirir.</summary>
        List<LastPassDTO> GetLastPassesYemekhane(int firmaId, int take);
        /// <summary>Last Passes Arac sorgularını getirir.</summary>
        List<LastPassDTO> GetLastPassesArac(int firmaId, int take);
        /// <summary>Validate işlemini gerçekleştirir.</summary>
        AuthUserDTO Validate(int firmaId, string user, string password);
        /// <summary>Canlı izleme giriş ekranı dropdown için firma bazlı kullanıcı adlarını getirir.</summary>
        List<string> GetKullaniciAdlariByFirma(int firmaId);
    }
}
