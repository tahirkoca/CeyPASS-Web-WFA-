using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class ResmiTatilService : IResmiTatilService
    {
        private readonly IResmiTatilRepository _repo;

        public ResmiTatilService(IResmiTatilRepository repo)
        {
            _repo = repo;
        }
        public void DoldurSabit(int baslangicYili, int bitisYili)
        {
            if (bitisYili < baslangicYili)
                throw new ArgumentException("Bitiş yılı, başlangıç yılından küçük olamaz.");

            _repo.DoldurSabit(baslangicYili, bitisYili);
        }
        public void KaydetTekil(DateTime tarih, string ad, decimal? calismaSaati)
        {
            if (string.IsNullOrWhiteSpace(ad))
                throw new ArgumentException("Resmi tatil adı boş olamaz.");
            if (calismaSaati.HasValue && calismaSaati.Value < 0)
                throw new ArgumentException("Çalışma saati negatif olamaz.");

            _repo.EkleVeyaGuncelle(tarih, ad.Trim(), calismaSaati);
        }
        public List<ResmiTatilDTO> GetList(int? yil = null) => _repo.GetList(yil);
        public void EkleVeyaGuncelle(DateTime tarih, string ad, decimal? calismaSaat)
        {
            _repo.EkleVeyaGuncelle(tarih, ad, calismaSaat);
        }
    }
}
