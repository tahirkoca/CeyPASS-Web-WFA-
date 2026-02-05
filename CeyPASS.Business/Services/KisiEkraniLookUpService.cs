using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.DataAccess.Abstractions;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class KisiEkraniLookupService : IKisiEkraniLookUpService
    {
        private readonly ICalismaStatuRepository _statuRepo;
        private readonly IDepartmanRepository _departmanRepo;
        private readonly IPozisyonRepository _pozisyonRepo;
        private readonly IIsyeriRepository _isyeriRepo;
        private readonly IFirmaRepository _firmaRepo;
        private readonly IBolumRepository _bolumRepo;

        public KisiEkraniLookupService(ICalismaStatuRepository statuRepo, IDepartmanRepository departmanRepo, IPozisyonRepository pozisyonRepo, IIsyeriRepository isyeriRepo, IFirmaRepository firmaRepo, IBolumRepository bolumRepo)
        {
            _statuRepo = statuRepo;
            _departmanRepo = departmanRepo;
            _pozisyonRepo = pozisyonRepo;
            _isyeriRepo = isyeriRepo;
            _firmaRepo = firmaRepo;
            _bolumRepo = bolumRepo;
        }
        public List<LookupItem> GetCalismaStatuleri() => _statuRepo.GetByFirma();
        public List<LookupItem> GetDepartmanlar() => _departmanRepo.GetByFirma();
        public List<LookupItem> GetPozisyonlar() => _pozisyonRepo.GetByFirma();
        public List<LookupItem> GetIsyerleri(int firmId) => _isyeriRepo.GetByFirma(firmId);
        public List<LookupItem> GetFirma(int firmId) => _firmaRepo.GetSingle(firmId);
        public List<LookupItem> GetBolumler(int firmId) => _bolumRepo.GetByFirma(firmId);
    }
}
