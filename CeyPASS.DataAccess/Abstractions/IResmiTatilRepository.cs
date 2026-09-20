using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Resmi tatil takvimi.</summary>
    public interface IResmiTatilRepository
    {
        /// <summary>Doldur Sabit işlemini gerçekleştirir.</summary>
        void DoldurSabit(int basYil, int bitYil);       
        /// <summary>Ekle Veya Guncelle işlemini gerçekleştirir.</summary>
        void EkleVeyaGuncelle(DateTime tarih, string ad, decimal? calismaSaat);        
        /// <summary>List sorgularını getirir.</summary>
        List<ResmiTatilDTO> GetList(int? yil = null);      
    }
}
