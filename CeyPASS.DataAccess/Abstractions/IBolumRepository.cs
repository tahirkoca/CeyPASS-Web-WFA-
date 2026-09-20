using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Firma altı bölüm lookup verileri.</summary>
    public interface IBolumRepository
    {
        /// <summary>By Firma sorgularını getirir.</summary>
        List<LookupItem> GetByFirma(int firmaId);
    }
}
