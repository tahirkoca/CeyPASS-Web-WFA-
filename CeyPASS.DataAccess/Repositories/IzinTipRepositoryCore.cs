using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class IzinTipRepositoryCore : IIzinTipRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public IzinTipRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public List<IzinTip> GetAktifIzinTipleri()
        {
            return _context.IzinTipleri
                .Where(x => x.AktifMi)
                .OrderBy(x => x.Adi)
                .Select(x => new IzinTip
                {
                    IzinTipId = x.IzinTipId,
                    Kod = x.Kod,
                    Ad = x.Adi,
                    UcretliMi = x.UcretliMi,
                    AktifMi = x.AktifMi,
                    SaatlikKullanilabilirMi = x.SaatlikKullanilabilirMi ?? false
                })
                .ToList();
        }

        public int? GetSaatlikKullanilabilirTipId()
        {
            return _context.IzinTipleri
                .Where(x => x.AktifMi && x.SaatlikKullanilabilirMi == true)
                .OrderBy(x => x.IzinTipId)
                .Select(x => (int?)x.IzinTipId)
                .FirstOrDefault();
        }
    }
}
