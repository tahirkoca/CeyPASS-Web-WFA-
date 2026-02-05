using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IResmiTatilRepository
    {
        void DoldurSabit(int basYil, int bitYil);       
        void EkleVeyaGuncelle(DateTime tarih, string ad, decimal? calismaSaat);        
        List<ResmiTatilDTO> GetList(int? yil = null);      
    }
}
