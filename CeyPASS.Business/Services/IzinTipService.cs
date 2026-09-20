using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    /// <summary>İzin tipi sorguları.</summary>
    public class IzinTipService:IIzinTipService
    {
        private readonly IIzinTipRepository _repo;

        public IzinTipService(IIzinTipRepository repo)
        {
            _repo = repo;
        }
        /// <inheritdoc />
        public List<IzinTip> GetAktif() => _repo.GetAktifIzinTipleri();
        /// <inheritdoc />
        public int? GetSaatlikIzinTipId() => _repo.GetSaatlikKullanilabilirTipId();
    }
}
