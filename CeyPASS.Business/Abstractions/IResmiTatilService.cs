using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Resmi tatil takvimi yönetimi.</summary>
    public interface IResmiTatilService
    {
        /// <summary>Sabit resmi tatilleri yıl aralığı için doldurur.</summary>
        void DoldurSabit(int baslangicYili, int bitisYili);

        /// <summary>Tek tarihli resmi tatil kaydeder.</summary>
        void KaydetTekil(DateTime tarih, string ad, decimal? calismaSaati);

        /// <summary>Yıla göre resmi tatil listesi.</summary>
        List<ResmiTatilDTO> GetList(int? yil = null);

        /// <summary>Tarih için ekle veya güncelle.</summary>
        void EkleVeyaGuncelle(DateTime tarih, string ad, decimal? calismaSaat);
    }
}
