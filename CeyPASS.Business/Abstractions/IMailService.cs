using System.Collections.Generic;
using System.Threading.Tasks;

namespace CeyPASS.Business.Abstractions
{
    public interface IMailService
    {
        Task<bool> SendEmailAsync(List<string> alicilar, string konu, string icerik, bool htmlMi = true);
    }
}
