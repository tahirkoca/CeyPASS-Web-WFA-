using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    /// <summary>Bölüm lookup sorguları.</summary>
    public class BolumRepositoryCore : IBolumRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public BolumRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        /// <summary>By Firma sorgularını getirir.</summary>
        public List<LookupItem> GetByFirma(int firmaId)
        {
            return _context.Bolumler
                .Where(b => b.FirmaId == firmaId)
                .OrderBy(b => b.BolumAdi)
                .Select(b => new LookupItem
                {
                    Id = b.BolumId ?? 0,
                    Ad = b.BolumAdi
                })
                .ToList();
        }
    }
}
