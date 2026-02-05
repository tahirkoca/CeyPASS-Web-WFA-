using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class IzinTipService:IIzinTipService
    {
        private readonly IIzinTipRepository _repo;

        public IzinTipService(IIzinTipRepository repo)
        {
            _repo = repo;
        }
        public List<IzinTip> GetAktif() => _repo.GetAktifIzinTipleri();
        public int? GetSaatlikIzinTipId() => _repo.GetSaatlikKullanilabilirTipId();
    }
}
