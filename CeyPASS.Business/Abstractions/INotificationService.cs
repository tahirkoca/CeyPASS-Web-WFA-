using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CeyPASS.Business.Abstractions
{
    public interface INotificationService
    {
        Task<bool> GuncellemeNotifikasyonuGonderAsync(GuncellemeNotifikasyonDTO guncellemeInfo, string? logoBase64 = null);
        Task<bool> OzelNotifikasyonGonderAsync(List<string> alicilar, string konu, string mesaj);
        List<string> AliciGrupGetir(string grupAdi);
        string OnizlemeHtmlOlustur(GuncellemeNotifikasyonDTO guncellemeInfo, string? logoBase64 = null);
    }
}
