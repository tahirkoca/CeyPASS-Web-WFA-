using System.Collections.Generic;
using System.Threading.Tasks;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Toplu/asenkron e-posta gönderimi.</summary>
    public interface IMailService
    {
        /// <summary>Alıcı listesine e-posta gönderir.</summary>
        Task<bool> SendEmailAsync(List<string> alicilar, string konu, string icerik, bool htmlMi = true);
    }
}
