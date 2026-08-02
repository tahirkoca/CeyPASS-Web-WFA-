using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class PersonelVardiyaYemekYetkiService : IPersonelVardiyaYemekYetkiService
    {
        private readonly IPersonelVardiyaYemekYetkiRepository _repo;

        public PersonelVardiyaYemekYetkiService(IPersonelVardiyaYemekYetkiRepository repo)
        {
            _repo = repo;
        }

        public bool FirmaHasSaatPenceresiAktif(int firmaId) => _repo.FirmaHasSaatPenceresiAktif(firmaId);

        public List<PersonelVardiyaYemekYetki> GetByCalismaSekliId(int calismaSekliId) =>
            _repo.GetByCalismaSekliId(calismaSekliId);

        public (bool ok, string error) Add(PersonelVardiyaYemekYetki item)
        {
            var err = Validate(item, excludeId: null);
            if (err != null)
                return (false, err);

            _repo.Insert(item);
            return (true, null);
        }

        public (bool ok, string error) Update(PersonelVardiyaYemekYetki item)
        {
            if (item == null || item.Id <= 0)
                return (false, "Güncellenecek kayıt seçilmedi.");

            var err = Validate(item, excludeId: item.Id);
            if (err != null)
                return (false, err);

            if (!_repo.Update(item))
                return (false, "Kayıt güncellenemedi.");

            return (true, null);
        }

        public bool Delete(int id) => _repo.Delete(id);

        private string Validate(PersonelVardiyaYemekYetki item, int? excludeId)
        {
            if (item == null)
                return "Kayıt bilgisi eksik.";

            if (item.CalismaSekliId <= 0)
                return "Önce bir vardiya seçin.";

            if (item.IsyeriId <= 0)
                return "İşyeri seçiniz.";

            if (item.YemekBitisSaati <= item.YemekBaslangicSaati)
                return "Yemek bitiş saati, başlangıç saatinden büyük olmalıdır.";

            if (_repo.ExistsForIsyeri(item.CalismaSekliId, item.IsyeriId, excludeId))
                return "Bu vardiya için seçili işyerinde zaten bir yemek saat penceresi tanımlı.";

            return null;
        }
    }
}
