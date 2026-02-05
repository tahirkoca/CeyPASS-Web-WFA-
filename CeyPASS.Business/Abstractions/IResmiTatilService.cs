using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IResmiTatilService
    {
        void DoldurSabit(int baslangicYili, int bitisYili);
        void KaydetTekil(DateTime tarih, string ad, decimal? calismaSaati);
        List<ResmiTatilDTO> GetList(int? yil = null);
        void EkleVeyaGuncelle(DateTime tarih, string ad, decimal? calismaSaat);
    }
}
