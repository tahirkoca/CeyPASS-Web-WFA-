using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>E-posta tabanlı güncelleme ve özel bildirimler.</summary>
    public interface INotificationService
    {
        /// <summary>Sürüm/güncelleme bildirim e-postası gönderir.</summary>
        Task<bool> GuncellemeNotifikasyonuGonderAsync(GuncellemeNotifikasyonDTO guncellemeInfo, string? logoBase64 = null);

        /// <summary>Belirtilen alıcılara özel konu/mesaj gönderir.</summary>
        Task<bool> OzelNotifikasyonGonderAsync(List<string> alicilar, string konu, string mesaj);

        /// <summary>Grup adına göre alıcı e-posta listesi.</summary>
        List<string> AliciGrupGetir(string grupAdi);

        /// <summary>Gönderim öncesi HTML önizleme üretir.</summary>
        string OnizlemeHtmlOlustur(GuncellemeNotifikasyonDTO guncellemeInfo, string? logoBase64 = null);
    }
}
